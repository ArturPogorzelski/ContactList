using ContactList.Authentication.Models;
using ContactList.Core.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ContactList.API.Midleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtConfig _jwtConfig;
        private readonly ILogger<JwtMiddleware> _logger; // Logger do rejestrowania błędów

        // Konstruktor przyjmujący następny element potoku, konfigurację JWT i logger
        public JwtMiddleware(RequestDelegate next, IOptions<JwtConfig> jwtConfig, ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _jwtConfig = jwtConfig.Value;
            _logger = logger;
        }

        // Metoda wywoływana przy każdym żądaniu HTTP
        public async Task InvokeAsync(HttpContext context, IUserService userService)
        {
            // Pobieranie tokena z nagłówka żądania
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                try
                {
                    // Handler dla tokenów JWT
                    var tokenHandler = new JwtSecurityTokenHandler();
                    // Klucz używany do weryfikacji podpisu JWT
                    var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

                    // Walidacja tokena z uwzględnieniem klucza, ważności tokenu i braku potrzeby weryfikacji wystawcy oraz odbiorcy
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true, // Weryfikacja ważności tokenu
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    // Ekstrakcja danych z tokena
                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "userId").Value);
                    var userRoles = jwtToken.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();

                    // Dołączenie informacji o użytkowniku i jego rolach do kontekstu żądania
                    context.Items["User"] = await userService.GetUserByIdAsync(userId);
                    context.Items["Roles"] = userRoles;
                }
                catch (SecurityTokenException ex)
                {
                    // Logowanie błędu podczas walidacji tokena i ustawienie odpowiedzi na kod 401 Unauthorized
                    _logger.LogError(ex, "Błąd podczas walidacji tokenu JWT.");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Nieprawidłowy token.");
                    return; // Przerwanie dalszego przetwarzania w potoku
                }
            }
            else
            {
                // Logowanie ostrzeżenia, gdy nie znaleziono tokena w nagłówku
                _logger.LogWarning("Brak tokenu JWT w nagłówku Authorization.");
                // Opcjonalne: zwrócenie odpowiedzi 401 Unauthorized lub pozwolenie na dalsze przetwarzanie żądania
            }

            // Przekazanie kontrolki do następnego middleware w potoku
            await _next(context);
        }
    }
}
