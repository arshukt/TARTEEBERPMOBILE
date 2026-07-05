using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TarteebErp.Domain.DTOs;
using TarteebErp.Application.Services;
using TarteebErp.Shared.Responses;

namespace TarteebErp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet("current-stock")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CurrentStockReportItemDto>>>> GetCurrentStockReport()
    {
        try
        {
            var result = await _reportService.GetCurrentStockReportAsync();
            return Ok(ApiResponse<IEnumerable<CurrentStockReportItemDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current stock report");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("sales")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SalesReportItemDto>>>> GetSalesReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            var result = await _reportService.GetSalesReportAsync(startDate, endDate);
            return Ok(ApiResponse<IEnumerable<SalesReportItemDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales report");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("purchases")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PurchasesReportItemDto>>>> GetPurchasesReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            var result = await _reportService.GetPurchasesReportAsync(startDate, endDate);
            return Ok(ApiResponse<IEnumerable<PurchasesReportItemDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting purchases report");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("customer-outstanding")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerOutstandingReportItemDto>>>> GetCustomerOutstandingReport()
    {
        try
        {
            var result = await _reportService.GetCustomerOutstandingReportAsync();
            return Ok(ApiResponse<IEnumerable<CustomerOutstandingReportItemDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer outstanding report");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("supplier-outstanding")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SupplierOutstandingReportItemDto>>>> GetSupplierOutstandingReport()
    {
        try
        {
            var result = await _reportService.GetSupplierOutstandingReportAsync();
            return Ok(ApiResponse<IEnumerable<SupplierOutstandingReportItemDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supplier outstanding report");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
}
