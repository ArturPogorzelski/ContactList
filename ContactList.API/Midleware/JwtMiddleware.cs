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
        private readonly ILogger<JwtMiddleware> _logger; // Logger for recording errors

        public JwtMiddleware(RequestDelegate next, IOptions<JwtConfig> jwtConfig, ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _jwtConfig = jwtConfig.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IUserService userService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false, // Weryfikacja wystawcy tokenu
                        ValidateAudience = false, // Weryfikacja odbiorcy tokenu
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "userId").Value);
                    var userRoles = jwtToken.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList(); // Pobranie ról

                    // Dołącz użytkownika i role do kontekstu żądania
                    context.Items["User"] = await userService.GetUserByIdAsync(userId);
                    context.Items["Roles"] = userRoles;
                }
                catch (SecurityTokenException ex)
                {
                    _logger.LogError(ex, "Błąd podczas walidacji tokenu JWT.");
                    context.Response.StatusCode = 401; // Unauthorized
                    await context.Response.WriteAsync("Nieprawidłowy token.");
                    return; // Zatrzymaj dalsze przetwarzanie żądania
                }
            }
            else
            {
                _logger.LogWarning("Brak tokenu JWT w nagłówku Authorization.");
                // Możesz tutaj zwrócić 401 Unauthorized lub zezwolić na dalsze przetwarzanie
            }

            await _next(context);
        }
    }
}
