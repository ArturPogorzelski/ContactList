using ContactList.Application.Commands;
using ContactList.Authentication.Services;
using ContactList.Core.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Handlers
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginRequestDto>
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginUserCommandHandler> _logger; // Dodajemy logger

        public LoginUserCommandHandler(IAuthService authService, ILogger<LoginUserCommandHandler> logger) // Wstrzykujemy ILogger
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<LoginRequestDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            try
            {

                var user = await _authService.GetUserByEmailAsync(request.Email);
                if (user == null || !_authService.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
                {
                    throw new UnauthorizedAccessException("Nieprawidłowy adres e-mail lub hasło.");
                }

                _logger.LogInformation("Użytkownik {Email} zalogował się pomyślnie.", request.Email); // Logowanie udanego logowania

                // Przygotowanie i zwrócenie DTO zawierającego token
                var token = await _authService.GenerateJwtToken(user); // Generowanie tokenu dla użytkownika

                return new LoginRequestDto { Token = token }; // Zakładając, że LoginRequestDto posiada właściwość Token
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Nieudana próba logowania dla użytkownika {Email}.", request.Email); // Logowanie nieudanej próby
                throw; // Ponownie rzucamy wyjątek, aby był obsłużony przez ErrorHandlingMiddleware
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas logowania użytkownika {Email}.", request.Email); // Logowanie innych błędów
                throw; // Ponownie rzucamy wyjątek, aby był obsłużony przez ErrorHandlingMiddleware
            }
        }
    }
}
