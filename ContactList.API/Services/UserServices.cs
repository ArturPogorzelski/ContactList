using AutoMapper;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using ContactList.Core.Exceptions;
using ContactList.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ContactList.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _passwordHasher; // Dodajemy hasher haseł
        
        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IMapper mapper,
            IConfiguration configuration,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserDto> CreateUserAsync(RegisterRequestDto registerRequestDto)
        {
            if (await _userRepository.GetByEmailAsync(registerRequestDto.Email) != null)
            {
                throw new BadRequestException("Użytkownik o podanym adresie email już istnieje.");
            }
            // Walidacja siły hasła (przykład z wyrażeniem regularnym)
            if (!Regex.IsMatch(registerRequestDto.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"))
            {
                throw new BadRequestException("Hasło nie spełnia wymagań dotyczących siły hasła. Hasło musi mieć conajmniej 8 znaków i zawierać co najmniej jedną dużą literę, jedną małą literę, jedną cyfrę i jeden znak specjalny.");
            }

            var user = _mapper.Map<User>(registerRequestDto);
            user.PasswordHash = _passwordHasher.HashPassword(user, registerRequestDto.Password);

            var role = await _roleRepository.GetByNameAsync("User"); // Changed from "Admin" to "User" for default role
            if (role == null)
            {
                throw new ApplicationException("Default role not found.");
            }
            user.UserRoles = new List<UserRole> { new UserRole { Role = role } };

            await _userRepository.AddAsync(user);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<string> AuthenticateUserAsync(LoginRequestDto loginRequestDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginRequestDto.Email);
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginRequestDto.Password) != PasswordVerificationResult.Success)
            {
                throw new ContactList.Core.Exceptions.UnauthorizedAccessException("Nieprawidłowy email lub hasło.");
            }

            // Generowanie tokenu JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtConfig:Secret"]);
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

            // Dodawanie ról do claims
            var roles = await _userRepository.GetUserRolesAsync(user.UserId);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtConfig:TokenLifetime"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> GetUserByEmailAsync(string  email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            return _mapper.Map<UserDto>(user);
        }

    }
}
