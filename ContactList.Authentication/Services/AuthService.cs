using ContactList.Authentication.Models;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using ContactList.Core.Interfaces;
using ContactList.Infrastructure.Data.Contexts;
using ContactList.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        // Konstruktor inicjujący wszystkie zależności
        public AuthService(IOptions<JwtConfig> jwtConfig, ContactListDbContext context, IRoleRepository roleRepository, IConfiguration configuration)
        {
            _jwtConfig = jwtConfig.Value;
            _context = context;
            _roleRepository = roleRepository;
            _configuration = configuration;
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
        public async Task<(bool isValid, string error)> RegisterUser(User user, string password, IEnumerable<string> roleNames)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return (false, "Email address is already in use.");
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = Convert.ToBase64String(passwordHash);
            user.PasswordSalt = passwordSalt;

            var roles = await _roleRepository.GetByNamesAsync(roleNames);
            if (roles.Count() != roleNames.Count())
            {
                return (false, "One or more invalid role names provided.");
            }

            user.UserRoles = roles.Select(r => new UserRole { Role = r }).ToList();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "User registered successfully.");
        }


        // Pobranie użytkownika na podstawie emaila
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role) // Dołącz role użytkownika
                                       .FirstOrDefaultAsync(u => u.Email == email);
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
        public async Task<string> AuthenticateUserAsync(LoginRequestDto loginRequestDto)
        {
            var user = await GetUserByEmailAsync(loginRequestDto.Email); // Używamy metody GetUserByEmailAsync
            if (user == null || !VerifyPassword(loginRequestDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new UnauthorizedAccessException("Nieprawidłowy adres e-mail lub hasło.");
            }

            // Generowanie tokenu JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtConfig:Secret"]);
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email) // Dodajemy email do claims
    };

            // Dodawanie ról do claims
            claims.AddRange(user.UserRoles.Select(role => new Claim(ClaimTypes.Role, role.Role.Name)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public (string Hash, byte[] Salt) HashPassword(string password)
        {
            using (var hmac = new HMACSHA512()) // Tworzy instancję algorytmu HMACSHA512
            {
                var salt = hmac.Key; // Generuje losową sól
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)); // Oblicza hash hasła z użyciem soli

                // Zwraca krotkę zawierającą hash (w formacie Base64) i sól
                return (Convert.ToBase64String(hash), salt);
            }
        }

    }
}
