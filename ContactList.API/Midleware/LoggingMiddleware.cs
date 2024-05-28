namespace ContactList.API.Midleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        // Konstruktor przyjmujący następny element potoku przetwarzania żądań i logger
        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // Metoda wywoływana przy każdym żądaniu HTTP
        public async Task InvokeAsync(HttpContext context)
        {
            // Logowanie nagłówków przychodzącego żądania
            _logger.LogInformation("Headers: " + string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}")));

            // Przekazanie kontroli do następnego middleware w potoku
            await _next(context);
        }
    }
}
