using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Domain;
using Server.DTOs.Requests;

namespace Server.Controllers;

[ApiController]
[Route("api/bank-accounts")]
[Authorize]
public class BankAccountController : ControllerBase
{
    private readonly IBankAccountService  _bankAccountService;
    private readonly IPortfolioService    _portfolioService;

    public BankAccountController(IBankAccountService bankAccountService, IPortfolioService portfolioService)
    {
        _bankAccountService = bankAccountService;
        _portfolioService   = portfolioService;
    }

    private int? GetUserId()
    {
        var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(str, out var id) ? id : null;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _bankAccountService.GetAllByUserId(userId.Value);
        return Ok(result.Payload);
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddBankAccountRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _bankAccountService.Add(userId.Value, request);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.Conflict => Conflict(result.Messages),
                _                   => StatusCode(500, result.Messages)
            };

        return StatusCode(201, result.Payload);
    }

    [HttpPost("{id}/deposit")]
    public async Task<IActionResult> Deposit(int id, AdjustBalanceRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _bankAccountService.Deposit(id, userId.Value, request);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.NotFound => NotFound(result.Messages),
                ResultType.Invalid  => BadRequest(result.Messages),
                _                   => StatusCode(500, result.Messages)
            };

        return Ok(result.Payload);
    }

    [HttpPost("{id}/withdraw")]
    public async Task<IActionResult> Withdraw(int id, AdjustBalanceRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _bankAccountService.Withdraw(id, userId.Value, request);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.NotFound => NotFound(result.Messages),
                ResultType.Invalid  => BadRequest(result.Messages),
                _                   => StatusCode(500, result.Messages)
            };

        return Ok(result.Payload);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _bankAccountService.Deactivate(id, userId.Value);

        if (!result.IsSuccess)
            return result.Type switch
            {
                ResultType.NotFound  => NotFound(result.Messages),
                ResultType.Forbidden => StatusCode(403, result.Messages),
                _                    => StatusCode(500, result.Messages)
            };

        return NoContent();
    }
}