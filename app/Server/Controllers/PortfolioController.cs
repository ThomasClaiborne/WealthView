using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Domain;

namespace Server.Controllers;

[ApiController]
[Route("api/portfolio")]
[Authorize]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;

    public PortfolioController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var result = await _portfolioService.GetByUserId(userId);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.NotFound => NotFound(result.Messages),
                _ => StatusCode(500, result.Messages)
            };

        return Ok(result.Payload);
    }

    [HttpGet("snapshots")]
    public async Task<IActionResult> GetSnapshotHistory()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var portfolioResult = await _portfolioService.GetByUserId(userId);
        if (!portfolioResult.IsSuccess)
            return portfolioResult.Type switch
            {
                ResultType.NotFound => NotFound(portfolioResult.Messages),
                _ => StatusCode(500, portfolioResult.Messages)
            };

        var result = await _portfolioService.GetSnapshotHistory(portfolioResult.Payload!.PortfolioId);
        return Ok(result.Payload);
    }
}