namespace ContactList.API.Midleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Headers: " + string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}")));
            await _next(context);
        }
    }
}
