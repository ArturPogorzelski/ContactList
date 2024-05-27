using AutoMapper;
using ContactList.API.Services;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using ContactList.Core.Exceptions;
using ContactList.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.UnitTests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _mapperMock = new Mock<IMapper>();
            _configurationMock = new Mock<IConfiguration>();
            _passwordHasherMock = new Mock<IPasswordHasher<User>>();
            
            _userService = new UserService(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _mapperMock.Object,
                _configurationMock.Object,
                _passwordHasherMock.Object
            );
        }

       

        [Fact]
        public async Task CreateUserAsync_ExistingEmail_ThrowsBadRequestException()
        {
            // Arrange
            var registerRequestDto = new RegisterRequestDto { Email = "test@example.com", Password = "password" };
            var existingUser = new User { UserId = 1, Email = "test@example.com" };

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(registerRequestDto.Email)).ReturnsAsync(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _userService.CreateUserAsync(registerRequestDto));
        }

        // ... (pozostała część klasy UserServiceTests)

        [Fact]
        public async Task AuthenticateUserAsync_ValidCredentials_ReturnsJwtToken()
        {
            // Arrange
            var loginRequestDto = new LoginRequestDto { Email = "test@example.com", Password = "password" };
            var user = new User { UserId = 1, Email = "test@example.com", PasswordHash = "hashedPassword", PasswordSalt = new byte[] { } };
            var userDto = new UserDto { UserId = 1, Email = "test@example.com" };
            var role = new Role { RoleId = 1, Name = "User" };
            user.UserRoles = new List<UserRole> { new UserRole { Role = role } };

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(loginRequestDto.Email)).ReturnsAsync(user);
            _passwordHasherMock.Setup(hasher => hasher.VerifyHashedPassword(user, user.PasswordHash, loginRequestDto.Password)).Returns(PasswordVerificationResult.Success);
            _configurationMock.SetupGet(config => config["JwtConfig:Secret"]).Returns("DbqB8l61z8P4tjt7QY7PLvDbqB8l61z8P4tjt7QY7PLv");
            _configurationMock.SetupGet(config => config["JwtConfig:TokenLifetime"]).Returns("60"); // 60 minut

            _userRepositoryMock.Setup(repo => repo.GetUserRolesAsync(user.UserId)).ReturnsAsync(new List<string> { "User" }); // Zwróć listę z rolą "User"

            // Act
            var result = await _userService.AuthenticateUserAsync(loginRequestDto);

            //// Act
            //var result = await _userService.AuthenticateUserAsync(loginRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // Możesz dodać bardziej szczegółowe asercje dotyczące zawartości tokenu JWT
        }

        [Fact]
        public async Task AuthenticateUserAsync_InvalidCredentials_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginRequestDto = new LoginRequestDto { Email = "test@example.com", Password = "wrongpassword" };
            var user = new User { UserId = 1, Email = "test@example.com", PasswordHash = "hashedPassword", PasswordSalt = new byte[] { } };

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(loginRequestDto.Email)).ReturnsAsync(user);
            _passwordHasherMock.Setup(hasher => hasher.VerifyHashedPassword(user, user.PasswordHash, loginRequestDto.Password)).Returns(PasswordVerificationResult.Failed);

            // Act & Assert
            await Assert.ThrowsAsync<ContactList.Core.Exceptions.UnauthorizedAccessException>(() => _userService.AuthenticateUserAsync(loginRequestDto));
        }

        [Fact]
        public async Task GetUserByIdAsync_ExistingUser_ReturnsUserDto()
        {
            // Arrange
            var userId = 1;
            var user = new User { UserId = userId, Email = "test@example.com" };
            var userDto = new UserDto { UserId = userId, Email = "test@example.com" };
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _mapperMock.Setup(mapper => mapper.Map<UserDto>(user)).Returns(userDto);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.Equal(userDto, result);
        }

        [Fact]
        public async Task GetUserByIdAsync_NonExistingUser_ThrowsNotFoundException()
        {
            // Arrange
            var userId = 1;
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _userService.GetUserByIdAsync(userId));
        }
    }
}
