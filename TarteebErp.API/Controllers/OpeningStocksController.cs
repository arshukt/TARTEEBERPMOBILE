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
public class OpeningStocksController : ControllerBase
{
    private readonly IOpeningStockService _openingStockService;
    private readonly ILogger<OpeningStocksController> _logger;

    public OpeningStocksController(IOpeningStockService openingStockService, ILogger<OpeningStocksController> logger)
    {
        _openingStockService = openingStockService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<OpeningStockDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        try
        {
            var result = await _openingStockService.GetPagedAsync(request);
            return Ok(ApiResponse<PagedResponse<OpeningStockDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting opening stocks");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<OpeningStockDto>>> GetById(int id)
    {
        try
        {
            var result = await _openingStockService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse.FailureResult("Opening stock not found"));
            return Ok(ApiResponse<OpeningStockDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting opening stock");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OpeningStockDto>>> Create([FromBody] CreateOpeningStockDto dto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _openingStockService.CreateAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<OpeningStockDto>.SuccessResult(result, "Opening stock created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.FailureResult(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating opening stock");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] UpdateOpeningStockDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse.FailureResult("Id mismatch"));
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _openingStockService.UpdateAsync(dto, currentUserId);
            return Ok(ApiResponse.SuccessResult("Opening stock updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.FailureResult(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating opening stock");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            await _openingStockService.DeleteAsync(id);
            return Ok(ApiResponse.SuccessResult("Opening stock deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting opening stock");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
}
