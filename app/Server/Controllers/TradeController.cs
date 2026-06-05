using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Domain;
using Server.DTOs.Requests;

namespace Server.Controllers;

[ApiController]
[Route("api/trades")]
[Authorize]
public class TradeController : ControllerBase
{
    private readonly ITradeService    _tradeService;
    private readonly IPortfolioService _portfolioService;

    public TradeController(ITradeService tradeService, IPortfolioService portfolioService)
    {
        _tradeService     = tradeService;
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

        var result = await _tradeService.GetAll(portfolioResult.Payload!.PortfolioId);
        return Ok(result.Payload);
    }

    [HttpPost("buy")]
    public async Task<IActionResult> Buy(BuyRequest request)
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

        var result = await _tradeService.Buy(portfolioResult.Payload!.PortfolioId, request);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.NotFound => NotFound(result.Messages),
                ResultType.Invalid  => BadRequest(result.Messages),
                _                  => StatusCode(500, result.Messages)
            };

        return StatusCode(201, result.Payload);
    }

    [HttpPost("sell")]
    public async Task<IActionResult> Sell(SellRequest request)
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

        var result = await _tradeService.Sell(portfolioResult.Payload!.PortfolioId, request);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.NotFound => NotFound(result.Messages),
                ResultType.Invalid  => BadRequest(result.Messages),
                _                  => StatusCode(500, result.Messages)
            };

        return StatusCode(201, result.Payload);
    }
}