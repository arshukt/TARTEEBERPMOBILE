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
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<CustomerDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        try
        {
            var result = await _customerService.GetPagedAsync(request);
            return Ok(ApiResponse<PagedResponse<CustomerDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerDto>>>> GetAll()
    {
        try
        {
            var result = await _customerService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<CustomerDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all customers");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetById(int id)
    {
        try
        {
            var result = await _customerService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse.FailureResult("Customer not found"));
            }
            return Ok(ApiResponse<CustomerDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Create([FromBody] CreateCustomerDto dto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _customerService.CreateAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<CustomerDto>.SuccessResult(result, "Customer created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse.FailureResult("Id mismatch"));
            }
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _customerService.UpdateAsync(dto, currentUserId);
            return Ok(ApiResponse.SuccessResult("Customer updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            await _customerService.DeleteAsync(id);
            return Ok(ApiResponse.SuccessResult("Customer deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
}
