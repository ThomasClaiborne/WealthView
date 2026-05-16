using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Domain;

namespace Server.Controllers;

[ApiController]
[Route("api/holdings")]
[Authorize]
public class HoldingController : ControllerBase
{
    private readonly IHoldingService  _holdingService;
    private readonly IPortfolioService _portfolioService;

    public HoldingController(IHoldingService holdingService, IPortfolioService portfolioService)
    {
        _holdingService   = holdingService;
        _portfolioService = portfolioService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var portfolioResult = await _portfolioService.GetByUserId(userId);
        if (!portfolioResult.IsSuccess)
            return portfolioResult.Type switch
            {
                ResultType.NotFound => NotFound(portfolioResult.Messages),
                _                  => StatusCode(500, portfolioResult.Messages)
            };

        var result = await _holdingService.GetAllByPortfolioId(portfolioResult.Payload!.PortfolioId);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.NotFound => NotFound(result.Messages),
                _                  => StatusCode(500, result.Messages)
            };

        return Ok(result.Payload);
    }
}