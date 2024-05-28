namespace ContactList.API.Midleware
{
    public class JwtDebugMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtDebugMiddleware> _logger;

        // Konstruktor inicjalizujący middleware z następnym elementem w potoku żądań i loggerem
        public JwtDebugMiddleware(RequestDelegate next, ILogger<JwtDebugMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // Metoda wywoływana przy każdym żądaniu. Obsługuje przekierowanie żądania do kolejnego middleware
        public async Task InvokeAsync(HttpContext context)
        {
            // Sprawdzenie, czy użytkownik jest uwierzytelniony
            if (context.User.Identity.IsAuthenticated)
            {
                // Jeśli tak, zbieranie i logowanie informacji o roszczeniach użytkownika
                var claims = context.User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                _logger.LogInformation("Authenticated User Claims: " + string.Join(", ", claims));
            }
            else
            {
                // Jeśli użytkownik nie jest uwierzytelniony, logowanie ostrzeżenia
                _logger.LogWarning("User is not authenticated.");
            }

            // Próba przekazania wykonania do następnego middleware w potoku
            await _next(context);
        }
    }
}
