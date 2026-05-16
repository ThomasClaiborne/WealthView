using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Domain;
using Server.DTOs.Requests;

namespace Server.Controllers;

[ApiController]
[Route("api/transfers")]
[Authorize]
public class FundTransferController : ControllerBase
{
    private readonly IFundTransferService _transferService;
    private readonly IPortfolioService    _portfolioService;

    public FundTransferController(IFundTransferService transferService, IPortfolioService portfolioService)
    {
        _transferService  = transferService;
        _portfolioService = portfolioService;
    }

    private int? GetUserId()
    {
        var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(str, out var id) ? id : null;
    }

    private async Task<int?> GetPortfolioId(int userId)
    {
        var result = await _portfolioService.GetByUserId(userId);
        return result.IsSuccess ? result.Payload!.PortfolioId : null;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var portfolioId = await GetPortfolioId(userId.Value);
        if (portfolioId is null) return NotFound("Portfolio not found.");

        var result = await _transferService.GetPending(portfolioId.Value);
        return Ok(result.Payload);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var portfolioId = await GetPortfolioId(userId.Value);
        if (portfolioId is null) return NotFound("Portfolio not found.");

        var result = await _transferService.GetHistory(portfolioId.Value);
        return Ok(result.Payload);
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> RequestDeposit(FundTransferRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var portfolioId = await GetPortfolioId(userId.Value);
        if (portfolioId is null) return NotFound("Portfolio not found.");

        var result = await _transferService.RequestDeposit(portfolioId.Value, request);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.NotFound => NotFound(result.Messages),
                ResultType.Invalid  => BadRequest(result.Messages),
                _                   => StatusCode(500, result.Messages)
            };

        return StatusCode(201, result.Payload);
    }

    [HttpPost("withdrawal")]
    public async Task<IActionResult> RequestWithdrawal(FundTransferRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var portfolioId = await GetPortfolioId(userId.Value);
        if (portfolioId is null) return NotFound("Portfolio not found.");

        var result = await _transferService.RequestWithdrawal(portfolioId.Value, request);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.NotFound => NotFound(result.Messages),
                ResultType.Invalid  => BadRequest(result.Messages),
                _                   => StatusCode(500, result.Messages)
            };

        return StatusCode(201, result.Payload);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var portfolioId = await GetPortfolioId(userId.Value);
        if (portfolioId is null) return NotFound("Portfolio not found.");

        var result = await _transferService.Approve(id, portfolioId.Value);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.NotFound => NotFound(result.Messages),
                ResultType.Invalid  => BadRequest(result.Messages),
                _                   => StatusCode(500, result.Messages)
            };

        return Ok(result.Payload);
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var portfolioId = await GetPortfolioId(userId.Value);
        if (portfolioId is null) return NotFound("Portfolio not found.");

        var result = await _transferService.Reject(id, portfolioId.Value);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.NotFound => NotFound(result.Messages),
                ResultType.Invalid  => BadRequest(result.Messages),
                _                   => StatusCode(500, result.Messages)
            };

        return Ok(result.Payload);
    }
}