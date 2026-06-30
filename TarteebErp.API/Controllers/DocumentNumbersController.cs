using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TarteebErp.Application.Services;
using TarteebErp.Shared.Responses;

namespace TarteebErp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentNumbersController : ControllerBase
{
    private readonly IDocumentNumberService _documentNumberService;
    private readonly ILogger<DocumentNumbersController> _logger;

    public DocumentNumbersController(
        IDocumentNumberService documentNumberService,
        ILogger<DocumentNumbersController> logger)
    {
        _documentNumberService = documentNumberService;
        _logger = logger;
    }

    [HttpGet("{documentType}/next")]
    public async Task<ActionResult<ApiResponse<string>>> GetNext(string documentType)
    {
        try
        {
            var nextNumber = await _documentNumberService.GetNextNumberAsync(documentType);
            return Ok(ApiResponse<string>.SuccessResult(nextNumber));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next document number for {DocumentType}", documentType);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.FailureResult("An error occurred"));
        }
    }
}
