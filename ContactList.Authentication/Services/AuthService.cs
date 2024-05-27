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
        private readonly JwtConfig _jwtConfig;
        private readonly ContactListDbContext _context;
        private readonly IRoleRepository _roleRepository;

        public AuthService(IOptions<JwtConfig> jwtConfig, ContactListDbContext context, IRoleRepository roleRepository)
        {
            _jwtConfig = jwtConfig.Value;
            _context = context;
            _roleRepository = roleRepository;
        }

        public async Task<string> GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", user.UserId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtConfig.ExpirationInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> ValidateUser(string email, string password)
        {
            // Znajdź użytkownika po adresie e-mail.
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            // Sprawdź hasło.
            return VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
        }


        public async Task<(bool isSuccess, string message)> RegisterUser(User user, string password, IEnumerable<string> roleNames)
        {
            // Sprawdzenie, czy użytkownik o podanym emailu już istnieje
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return (false, "Email address is already in use.");
            }

            // Hashowanie hasła przed zapisem
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = Convert.ToBase64String(passwordHash); // Zapisujemy hash w formacie Base64
            user.PasswordSalt = passwordSalt;

            // Pobranie ról na podstawie podanych nazw
            var roles = await _roleRepository.GetByNamesAsync(roleNames);
            if (roles.Count() != roleNames.Count())
            {
                return (false, "One or more invalid role names provided.");
            }

            // Przypisanie ról do użytkownika
            user.UserRoles = roles.Select(r => new UserRole { Role = r }).ToList();

            // Dodanie użytkownika do bazy danych
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Rejestracja zakończona sukcesem
            return (true, "User registered successfully.");
        }
        public async Task<User> GetUserByEmailAsync(string email) 
        {
          var user = await _context.Users.Where(x => x.Email == email).FirstOrDefaultAsync();

            return user;
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // 1. Create an instance of the HMACSHA512 algorithm
            using (var hmac = new HMACSHA512())
            {
                // 2. Generate a random salt (unique for each user)
                passwordSalt = hmac.Key;

                // 3. Compute the hash of the password concatenated with the salt
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

       
       
        public bool VerifyPassword(string actualPassword, string hashedPassword, byte[] salt)
        {
            using (var hmac = new HMACSHA512(salt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(actualPassword));
                var computedHashString = Convert.ToBase64String(computedHash); // Konwersja do Base64

                return computedHashString.ToLower() == hashedPassword.ToLower(); // Porównanie z konwersją na małe litery
            }
        }
    }
}
