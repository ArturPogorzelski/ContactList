using AutoMapper;

using ContactList.Application.Commands;
using ContactList.Authentication.Services;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using ContactList.Core.Exceptions;
using ContactList.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Handlers
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAuthService _authService;

        public RegisterUserCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository, IMapper mapper, IPasswordHasher<User> passwordHasher, IAuthService authService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _authService = authService;
        }

        public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Walidacja czy użytkownik o podanym emailu już istnieje
            if (await _userRepository.GetByEmailAsync(request.User.Email) != null)
            {
                throw new BadRequestException("Użytkownik o podanym adresie email już istnieje.");
            }

            // Mapowanie DTO na encję User
            var user = _mapper.Map<User>(request);

            // Hashowanie hasła
            user.PasswordHash = _passwordHasher.HashPassword(user, request.User.Password);

            //// Przypisanie roli (domyślnie "User")
            //var role = await _roleRepository.GetByNameAsync("User");
            //if (role == null)
            //{
            //    throw new ApplicationException("Rola domyślna nie została znaleziona.");
            //}
            //user.UserRoles = new List<UserRole> { new UserRole { Role = role } };

            // Dodanie użytkownika do bazy danych
            await _authService.RegisterUser(user, request.User.Password, new[] { "Admin", "User" });

            // Mapowanie encji User z powrotem na DTO
            return _mapper.Map<UserDto>(user);
        }
    }
}
