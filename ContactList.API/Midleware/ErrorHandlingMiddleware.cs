
using ContactList.Core.Exceptions;
using System.Net;
using System.Text.Json;

namespace ContactList.API.Midleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        // Konstruktor inicjalizujący middleware z następnym elementem w potoku żądań i loggerem
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // Metoda wywoływana przy każdym żądaniu. Obsługuje przekierowanie żądania do kolejnego middleware
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Próba przekazania wykonania do następnego middleware w potoku
                await _next(context);
            }
            catch (Exception ex)
            {
                // Przechwytuje wyjątki pojawiające się w dalszej części potoku i przekierowuje do metody obsługi błędów
                await HandleExceptionAsync(context, ex);
            }
        }

        // Prywatna metoda do obsługi błędów, wywoływana, gdy w potoku wystąpi wyjątek
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Logowanie wystąpienia wyjątku
            _logger.LogError(exception, "Wystąpił błąd podczas przetwarzania żądania.");

            var response = context.Response;
            response.ContentType = "application/json";

            // Rozgałęzienie w zależności od typu wyjątku, aby odpowiednio ustawić status odpowiedzi i wiadomość
            switch (exception)
            {
                case ValidationException validationException:
                    // Obsługa wyjątków walidacji, zwracających kod 400 - Bad Request
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        StatusCode = response.StatusCode,
                        Message = "Błąd walidacji.",
                        Errors = validationException.Errors
                    }));
                    break;
                case NotFoundException _:
                    // Obsługa wyjątku związanych z nieznalezieniem zasobu, zwracających kod 404 - Not Found
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    await response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        StatusCode = response.StatusCode,
                        Message = "Nie znaleziono zasobu."
                    }));
                    break;
                case ContactList.Core.Exceptions.UnauthorizedAccessException _:
                    // Obsługa wyjątku związanego z brakiem autoryzacji, zwracającego kod 401 - Unauthorized
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        StatusCode = response.StatusCode,
                        Message = "Brak autoryzacji."
                    }));
                    break;
                default:
                    // Domyślne zachowanie dla nieprzewidzianych wyjątków, zwracających kod 500 - Internal Server Error
                    var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        StatusCode = response.StatusCode,
                        Message = isDevelopment ? exception.Message : "Wystąpił błąd wewnętrzny serwera."
                    }));
                    break;
            }
        }
    }
}
