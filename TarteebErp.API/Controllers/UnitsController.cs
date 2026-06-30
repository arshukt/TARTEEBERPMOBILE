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
public class UnitsController : ControllerBase
{
    private readonly IUnitService _unitService;
    private readonly ILogger<UnitsController> _logger;

    public UnitsController(IUnitService unitService, ILogger<UnitsController> logger)
    {
        _unitService = unitService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<UnitDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        try
        {
            var result = await _unitService.GetPagedAsync(request);
            return Ok(ApiResponse<PagedResponse<UnitDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting units");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
    
    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UnitDto>>>> GetAll()
    {
        try
        {
            var result = await _unitService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<UnitDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all units");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UnitDto>>> GetById(int id)
    {
        try
        {
            var result = await _unitService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse.FailureResult("Unit not found"));
            }
            return Ok(ApiResponse<UnitDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UnitDto>>> Create([FromBody] CreateUnitDto dto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _unitService.CreateAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<UnitDto>.SuccessResult(result, "Unit created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating unit");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] UpdateUnitDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse.FailureResult("Id mismatch"));
            }
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _unitService.UpdateAsync(dto, currentUserId);
            return Ok(ApiResponse.SuccessResult("Unit updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            await _unitService.DeleteAsync(id);
            return Ok(ApiResponse.SuccessResult("Unit deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
}
