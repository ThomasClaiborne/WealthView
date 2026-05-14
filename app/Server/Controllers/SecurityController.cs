using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Domain;

namespace Server.Controllers;

[ApiController]
[Route("api/securities")]
[Authorize]
public class SecurityController : ControllerBase
{
    private readonly ISecurityService _securityService;

    public SecurityController(ISecurityService securityService)
    {
        _securityService = securityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _securityService.GetAll();

        if (!result.IsSuccess)
            return StatusCode(500, result.Messages);

        return Ok(result.Payload);
    }

    [HttpGet("{ticker}")]
    public async Task<IActionResult> GetByTicker(string ticker)
    {
        var result = await _securityService.GetByTicker(ticker);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.NotFound => NotFound(result.Messages),
                _                  => StatusCode(500, result.Messages)
            };

        return Ok(result.Payload);
    }
}