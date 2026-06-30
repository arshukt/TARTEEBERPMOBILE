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
public class PurchaseReturnsController : ControllerBase
{
    private readonly IPurchaseReturnService _purchaseReturnService;
    private readonly ILogger<PurchaseReturnsController> _logger;

    public PurchaseReturnsController(IPurchaseReturnService purchaseReturnService, ILogger<PurchaseReturnsController> logger)
    {
        _purchaseReturnService = purchaseReturnService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<PurchaseReturnDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        try
        {
            var result = await _purchaseReturnService.GetPagedAsync(request);
            return Ok(ApiResponse<PagedResponse<PurchaseReturnDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting purchase returns");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PurchaseReturnDto>>> GetById(int id)
    {
        try
        {
            var result = await _purchaseReturnService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse.FailureResult("PurchaseReturn not found"));
            return Ok(ApiResponse<PurchaseReturnDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting purchase return");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PurchaseReturnDto>>> Create([FromBody] CreatePurchaseReturnDto dto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _purchaseReturnService.CreateAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PurchaseReturnDto>.SuccessResult(result, "PurchaseReturn created successfully"));
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
            _logger.LogError(ex, "Error creating purchase return");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] UpdatePurchaseReturnDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse.FailureResult("Id mismatch"));
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _purchaseReturnService.UpdateAsync(dto, currentUserId);
            return Ok(ApiResponse.SuccessResult("PurchaseReturn updated successfully"));
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
            _logger.LogError(ex, "Error updating purchase return");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            await _purchaseReturnService.DeleteAsync(id);
            return Ok(ApiResponse.SuccessResult("PurchaseReturn deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting purchase return");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SaleReturnsController : ControllerBase
{
    private readonly ISaleReturnService _saleReturnService;
    private readonly ILogger<SaleReturnsController> _logger;

    public SaleReturnsController(ISaleReturnService saleReturnService, ILogger<SaleReturnsController> logger)
    {
        _saleReturnService = saleReturnService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<SaleReturnDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        try
        {
            var result = await _saleReturnService.GetPagedAsync(request);
            return Ok(ApiResponse<PagedResponse<SaleReturnDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sale returns");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SaleReturnDto>>> GetById(int id)
    {
        try
        {
            var result = await _saleReturnService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse.FailureResult("SaleReturn not found"));
            return Ok(ApiResponse<SaleReturnDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sale return");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SaleReturnDto>>> Create([FromBody] CreateSaleReturnDto dto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _saleReturnService.CreateAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<SaleReturnDto>.SuccessResult(result, "SaleReturn created successfully"));
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
            _logger.LogError(ex, "Error creating sale return");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] UpdateSaleReturnDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse.FailureResult("Id mismatch"));
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _saleReturnService.UpdateAsync(dto, currentUserId);
            return Ok(ApiResponse.SuccessResult("SaleReturn updated successfully"));
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
            _logger.LogError(ex, "Error updating sale return");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            await _saleReturnService.DeleteAsync(id);
            return Ok(ApiResponse.SuccessResult("SaleReturn deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sale return");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockAdjustmentsController : ControllerBase
{
    private readonly IStockAdjustmentService _stockAdjustmentService;
    private readonly ILogger<StockAdjustmentsController> _logger;

    public StockAdjustmentsController(IStockAdjustmentService stockAdjustmentService, ILogger<StockAdjustmentsController> logger)
    {
        _stockAdjustmentService = stockAdjustmentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<StockAdjustmentDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        try
        {
            var result = await _stockAdjustmentService.GetPagedAsync(request);
            return Ok(ApiResponse<PagedResponse<StockAdjustmentDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock adjustments");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<StockAdjustmentDto>>> GetById(int id)
    {
        try
        {
            var result = await _stockAdjustmentService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse.FailureResult("StockAdjustment not found"));
            return Ok(ApiResponse<StockAdjustmentDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock adjustment");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StockAdjustmentDto>>> Create([FromBody] CreateStockAdjustmentDto dto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _stockAdjustmentService.CreateAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<StockAdjustmentDto>.SuccessResult(result, "StockAdjustment created successfully"));
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
            _logger.LogError(ex, "Error creating stock adjustment");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] UpdateStockAdjustmentDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse.FailureResult("Id mismatch"));
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _stockAdjustmentService.UpdateAsync(dto, currentUserId);
            return Ok(ApiResponse.SuccessResult("StockAdjustment updated successfully"));
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
            _logger.LogError(ex, "Error updating stock adjustment");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            await _stockAdjustmentService.DeleteAsync(id);
            return Ok(ApiResponse.SuccessResult("StockAdjustment deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stock adjustment");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<PaymentDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        try
        {
            var result = await _paymentService.GetPagedAsync(request);
            return Ok(ApiResponse<PagedResponse<PaymentDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetById(int id)
    {
        try
        {
            var result = await _paymentService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse.FailureResult("Payment not found"));
            return Ok(ApiResponse<PaymentDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Create([FromBody] CreatePaymentDto dto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _paymentService.CreateAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PaymentDto>.SuccessResult(result, "Payment created successfully"));
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
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] UpdatePaymentDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse.FailureResult("Id mismatch"));
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _paymentService.UpdateAsync(dto, currentUserId);
            return Ok(ApiResponse.SuccessResult("Payment updated successfully"));
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
            _logger.LogError(ex, "Error updating payment");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            await _paymentService.DeleteAsync(id);
            return Ok(ApiResponse.SuccessResult("Payment deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockTransfersController : ControllerBase
{
    private readonly IStockTransferService _stockTransferService;
    private readonly ILogger<StockTransfersController> _logger;

    public StockTransfersController(IStockTransferService stockTransferService, ILogger<StockTransfersController> logger)
    {
        _stockTransferService = stockTransferService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<StockTransferDto>>>> GetPaged([FromQuery] PagedRequest request)
    {
        try
        {
            var result = await _stockTransferService.GetPagedAsync(request);
            return Ok(ApiResponse<PagedResponse<StockTransferDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock transfers");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<StockTransferDto>>> GetById(int id)
    {
        try
        {
            var result = await _stockTransferService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse.FailureResult("StockTransfer not found"));
            return Ok(ApiResponse<StockTransferDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock transfer");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StockTransferDto>>> Create([FromBody] CreateStockTransferDto dto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _stockTransferService.CreateAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<StockTransferDto>.SuccessResult(result, "StockTransfer created successfully"));
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
            _logger.LogError(ex, "Error creating stock transfer");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] UpdateStockTransferDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse.FailureResult("Id mismatch"));
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _stockTransferService.UpdateAsync(dto, currentUserId);
            return Ok(ApiResponse.SuccessResult("StockTransfer updated successfully"));
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
            _logger.LogError(ex, "Error updating stock transfer");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            await _stockTransferService.DeleteAsync(id);
            return Ok(ApiResponse.SuccessResult("StockTransfer deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stock transfer");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.FailureResult("An error occurred"));
        }
    }
}
