using AutoMapper;
using ContactList.Application.Commands;
using ContactList.Core.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContactList.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthCQRSController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IValidator<LoginRequestDto> _loginValidator;
        private readonly IValidator<RegisterRequestDto> _registerValidator;

        public AuthCQRSController(IMediator mediator, IValidator<LoginRequestDto> loginValidator, IValidator<RegisterRequestDto> registerValidator, IMapper mapper)
        {
            _mediator = mediator;
            _loginValidator = loginValidator;
            _registerValidator = registerValidator;
            _mapper = mapper;
        }

        [HttpPost("registerCQRS")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
        {
            var validationResult = await _registerValidator.ValidateAsync(registerRequestDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors); // Zwraca BadRequest z błędami walidacji

            var command = _mapper.Map<RegisterUserCommand>(registerRequestDto);

            try
            {
                var userDto = await _mediator.Send(command);
                return Ok(new { message = "Użytkownik został zarejestrowany." }); // Rejestracja udana
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message }); // Wystąpił błąd, zwracamy komunikat
            }
        }

        [HttpPost("loginCQRS")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            var validationResult = await _loginValidator.ValidateAsync(loginRequestDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors); // Zwraca BadRequest z błędami walidacji

            try
            {
                var command = _mapper.Map<LoginUserCommand>(loginRequestDto);
                //var command = _mapper.Map<LoginUserCommand>(loginDto);
                var result = await _mediator.Send(command);

                //var token = await _mediator.Send(loginRequestDto); // Wywołanie handlera logowania poprzez mediator
                if (string.IsNullOrEmpty(result.Token))
                    return Unauthorized(new { message = "Unauthorized" });

                return Ok(new { token = result.Token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wystąpił błąd wewnętrzny serwera. Message: "+ ex.Message); // Inny błąd
            }
        }
    }
}
