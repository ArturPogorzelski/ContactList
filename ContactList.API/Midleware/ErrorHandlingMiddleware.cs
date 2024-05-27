
using ContactList.Core.Exceptions;
using System.Net;
using System.Text.Json;

namespace ContactList.API.Midleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Wystąpił błąd podczas przetwarzania żądania.");

            var response = context.Response;
            response.ContentType = "application/json";

            switch (exception)
            {
                case ValidationException validationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        StatusCode = response.StatusCode,
                        Message = "Błąd walidacji.",
                        Errors = validationException.Errors
                    }));
                    break;
                case NotFoundException _:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    await response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        StatusCode = response.StatusCode,
                        Message = "Nie znaleziono zasobu."
                    }));
                    break;
                case ContactList.Core.Exceptions.UnauthorizedAccessException _:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        StatusCode = response.StatusCode,
                        Message = "Brak autoryzacji."
                    }));
                    break;
                default:
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
