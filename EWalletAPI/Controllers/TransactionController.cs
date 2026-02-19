using EWalletAPI.Helpers;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWalletAPI.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
[Produces("application/json")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Get the current user's transaction history (paginated).
    /// Query params: page (default 1), pageSize (default 10, max 100)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<PaginatedTransactionsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = ClaimsHelper.GetUserId(User);
        var result = await _transactionService.GetUserTransactionsAsync(userId, page, pageSize);

        return result.Success ? Ok(result) : NotFound(result);
    }
}
