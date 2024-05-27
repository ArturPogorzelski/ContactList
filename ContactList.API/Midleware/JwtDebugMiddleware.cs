namespace ContactList.API.Midleware
{
    public class JwtDebugMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtDebugMiddleware> _logger;

        public JwtDebugMiddleware(RequestDelegate next, ILogger<JwtDebugMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var claims = context.User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                _logger.LogInformation("Authenticated User Claims: " + string.Join(", ", claims));
            }
            else
            {
                _logger.LogWarning("User is not authenticated.");
            }

            await _next(context);
        }
    }
}
