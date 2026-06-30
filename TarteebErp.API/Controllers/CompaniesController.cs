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
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(ICompanyService companyService, ILogger<CompaniesController> logger)
    {
        _companyService = companyService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<CompanyDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        try
        {
            var result = await _companyService.GetPagedAsync(request);
            return Ok(ApiResponse<PagedResponse<CompanyDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting companies");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
    
    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CompanyDto>>>> GetAll()
    {
        try
        {
            var result = await _companyService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<CompanyDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all companies");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("first")]
    public async Task<ActionResult<ApiResponse<CompanyDto?>>> GetFirst()
    {
        try
        {
            var result = await _companyService.GetFirstAsync();
            return Ok(ApiResponse<CompanyDto?>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting first company");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> GetById(int id)
    {
        try
        {
            var result = await _companyService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse.FailureResult("Company not found"));
            }
            return Ok(ApiResponse<CompanyDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> Create([FromBody] CreateCompanyDto dto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _companyService.CreateAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<CompanyDto>.SuccessResult(result, "Company created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] UpdateCompanyDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse.FailureResult("Id mismatch"));
            }
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _companyService.UpdateAsync(dto, currentUserId);
            return Ok(ApiResponse.SuccessResult("Company updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            await _companyService.DeleteAsync(id);
            return Ok(ApiResponse.SuccessResult("Company deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
}