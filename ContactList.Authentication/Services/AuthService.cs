using ContactList.Authentication.Models;
using ContactList.Core.Entities;
using ContactList.Core.Interfaces;
using ContactList.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Authentication.Services
{
    public class AuthService : IAuthService
    {
        // Wstrzyknięte zależności konfiguracji JWT, kontekstu bazy danych i repozytorium ról
        private readonly JwtConfig _jwtConfig;
        private readonly ContactListDbContext _context;
        private readonly IRoleRepository _roleRepository;

        // Konstruktor inicjujący wszystkie zależności
        public AuthService(IOptions<JwtConfig> jwtConfig, ContactListDbContext context, IRoleRepository roleRepository)
        {
            _jwtConfig = jwtConfig.Value;
            _context = context;
            _roleRepository = roleRepository;
        }

        // Generowanie tokena JWT dla zalogowanego użytkownika
        public async Task<string> GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret)); // Klucz używany do podpisu JWT
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); // Poświadczenia używane do podpisu

            var claims = new[] // Tworzenie roszczeń dołączonych do tokena
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email), // Subiekt tokena
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unikalny identyfikator tokena
            new Claim("userId", user.UserId.ToString()) // Identyfikator użytkownika
        };

            var token = new JwtSecurityToken( // Tworzenie tokena JWT
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtConfig.ExpirationInMinutes), // Ustawienie czasu wygaśnięcia
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token); // Zwracanie sformatowanego tokena
        }

        // Walidacja danych logowania użytkownika
        public async Task<bool> ValidateUser(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email); // Wyszukiwanie użytkownika po emailu
            if (user == null) return false; // Brak użytkownika o podanym emailu

            return VerifyPassword(password, user.PasswordHash, user.PasswordSalt); // Sprawdzanie poprawności hasła
        }

        // Rejestracja nowego użytkownika
        public async Task<(bool isSuccess, string message)> RegisterUser(User user, string password, IEnumerable<string> roleNames)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email)) // Sprawdzenie czy użytkownik o danym emailu już istnieje
            {
                return (false, "Email address is already in use.");
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt); // Hashowanie hasła
            user.PasswordHash = Convert.ToBase64String(passwordHash); // Zapis hasła w formacie Base64
            user.PasswordSalt = passwordSalt;

            var roles = await _roleRepository.GetByNamesAsync(roleNames); // Pobranie ról na podstawie nazw
            if (roles.Count() != roleNames.Count()) // Sprawdzenie czy wszystkie role są poprawne
            {
                return (false, "One or more invalid role names provided.");
            }

            user.UserRoles = roles.Select(r => new UserRole { Role = r }).ToList(); // Przypisanie ról do użytkownika

            _context.Users.Add(user); // Dodanie użytkownika do bazy danych
            await _context.SaveChangesAsync(); // Zapis zmian

            return (true, "User registered successfully."); // Rejestracja zakończona sukcesem
        }

        // Pobranie użytkownika na podstawie emaila
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        // Tworzenie hasha hasła
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512()) // Użycie algorytmu HMACSHA512 do generacji hasha
            {
                passwordSalt = hmac.Key; // Użycie klucza jako soli
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)); // Generacja hasha
            }
        }

        // Weryfikacja hasła
        public bool VerifyPassword(string actualPassword, string hashedPassword, byte[] salt)
        {
            using (var hmac = new HMACSHA512(salt)) // Użycie soli do odtworzenia hasha
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(actualPassword)); // Obliczenie oczekiwanego hasha
                var computedHashString = Convert.ToBase64String(computedHash); // Konwersja do formatu Base64

                return computedHashString.ToLower() == hashedPassword.ToLower(); // Porównanie hasha
            }
        }
    }
}
