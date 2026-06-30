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
public class BrandsController : ControllerBase
{
    private readonly IBrandService _brandService;
    private readonly ILogger<BrandsController> _logger;

    public BrandsController(IBrandService brandService, ILogger<BrandsController> logger)
    {
        _brandService = brandService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<BrandDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        try
        {
            var result = await _brandService.GetPagedAsync(request);
            return Ok(ApiResponse<PagedResponse<BrandDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting brands");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
    
    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BrandDto>>>> GetAll()
    {
        try
        {
            var result = await _brandService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<BrandDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all brands");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<BrandDto>>> GetById(int id)
    {
        try
        {
            var result = await _brandService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse.FailureResult("Brand not found"));
            }
            return Ok(ApiResponse<BrandDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting brand");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BrandDto>>> Create([FromBody] CreateBrandDto dto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _brandService.CreateAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<BrandDto>.SuccessResult(result, "Brand created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating brand");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] UpdateBrandDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse.FailureResult("Id mismatch"));
            }
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _brandService.UpdateAsync(dto, currentUserId);
            return Ok(ApiResponse.SuccessResult("Brand updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating brand");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            await _brandService.DeleteAsync(id);
            return Ok(ApiResponse.SuccessResult("Brand deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting brand");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
}
