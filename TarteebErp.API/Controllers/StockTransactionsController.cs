using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TarteebErp.Application.DTOs;
using TarteebErp.Application.Services;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockTransactionsController : ControllerBase
{
    private readonly IStockTransactionService _stockTransactionService;
    private readonly ILogger<StockTransactionsController> _logger;

    public StockTransactionsController(IStockTransactionService stockTransactionService, ILogger<StockTransactionsController> logger)
    {
        _stockTransactionService = stockTransactionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<StockTransactionDto>>>> GetPaged([FromQuery] PagedRequest request, [FromQuery] int? itemId = null)
    {
        try
        {
            var result = await _stockTransactionService.GetPagedAsync(request, itemId);
            return Ok(ApiResponse<PagedResponse<StockTransactionDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock transactions");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
}
