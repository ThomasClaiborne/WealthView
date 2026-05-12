using Microsoft.AspNetCore.Mvc;
using Server.Domain;
using Server.DTOs.Requests;

namespace Server.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.Register(request);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.Invalid => BadRequest(result.Messages),
                _                  => StatusCode(500, result.Messages)
            };

        return StatusCode(201, result.Payload);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.Login(request);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.NotFound => Unauthorized(result.Messages),
                ResultType.Invalid  => Unauthorized(result.Messages),
                _                   => StatusCode(500, result.Messages)
            };

        return Ok(result.Payload);
    }
}