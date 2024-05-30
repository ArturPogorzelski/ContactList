using AutoMapper;
using ContactList.Authentication.Services;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using ContactList.Core.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactList.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<RegisterRequestDto> _registerValidator;

    public AuthController(IAuthService authService, IMapper mapper, IValidator<LoginRequestDto> loginValidator, IValidator<RegisterRequestDto> registerValidator, IUserService userService)
    {
        _authService = authService;
        _mapper = mapper;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
        _userService = userService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
    {
        var validationResult = await _registerValidator.ValidateAsync(registerRequestDto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var user = _mapper.Map<User>(registerRequestDto);
        var (isValid, error) = await _authService.RegisterUser(user, registerRequestDto.Password, new[] { "Admin", "User" });
        if (!isValid)
            return BadRequest(new { message = error });

        return Ok(new { message = "Użytkownik został zarejestrowany." });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> Login([FromBody] LoginRequestDto loginRequestDto)
    {
        var validationResult = await _loginValidator.ValidateAsync(loginRequestDto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        // Poprawione wywołanie metody AuthenticateUserAsync
        var user = await _authService.GetUserByEmailAsync(loginRequestDto.Email); // Pobranie użytkownika na podstawie emaila
        //var user = _mapper.Map<User>(loginRequestDto);
        if (user == null || !_authService.VerifyPassword(loginRequestDto.Password, user.PasswordHash, user.PasswordSalt))
        {
            return Unauthorized();
        }

        var token = await _authService.GenerateJwtToken(user); // Generowanie tokenu dla użytkownika
        return Ok(new { token });
    }
}