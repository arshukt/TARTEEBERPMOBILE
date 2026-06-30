using AutoMapper;
using TarteebErp.Application.DTOs;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Shared.Enums;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public class OpeningStockService : IOpeningStockService
{
    private readonly IOpeningStockRepository _openingStockRepository;
    private readonly IStockTransactionRepository _stockTransactionRepository;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IMapper _mapper;

    public OpeningStockService(
        IOpeningStockRepository openingStockRepository,
        IStockTransactionRepository stockTransactionRepository,
        IDocumentNumberService documentNumberService,
        IMapper mapper)
    {
        _openingStockRepository = openingStockRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _documentNumberService = documentNumberService;
        _mapper = mapper;
    }

    public async Task<PagedResponse<OpeningStockDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _openingStockRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _openingStockRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<OpeningStockDto>
        {
            Items = _mapper.Map<List<OpeningStockDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OpeningStockDto?> GetByIdAsync(int id)
    {
        // We'll need to load with details here - let's extend the repository later
        // For now, just get the header
        var openingStock = await _openingStockRepository.GetByIdAsync(id);
        return _mapper.Map<OpeningStockDto>(openingStock);
    }

    public async Task<OpeningStockDto> CreateAsync(CreateOpeningStockDto dto, int currentUserId)
    {
        ValidateOpeningStock(dto);
        dto.OpeningStockNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.OpeningStock,
            dto.OpeningStockNumber);

        var openingStock = _mapper.Map<OpeningStock>(dto);
        openingStock.CreatedAt = DateTime.UtcNow;
        openingStock.CreatedBy = currentUserId;

        await _openingStockRepository.AddAsync(openingStock);

        // Create stock transactions for each item
        foreach (var detail in openingStock.OpeningStockDetails)
        {
            await _stockTransactionRepository.AddTransactionAsync(new StockTransaction
            {
                ItemId = detail.ItemId,
                TransactionType = StockTransactionType.OpeningStock,
                QuantityIn = detail.Quantity,
                QuantityOut = 0,
                ReferenceId = openingStock.Id,
                ReferenceType = "OpeningStock",
                Notes = $"Opening stock {dto.OpeningStockNumber}: {dto.Notes}",
                TransactionDate = dto.Date,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            });
        }

        return _mapper.Map<OpeningStockDto>(openingStock);
    }

    public async Task UpdateAsync(UpdateOpeningStockDto dto, int currentUserId)
    {
        var openingStock = await _openingStockRepository.GetByIdAsync(dto.Id);
        if (openingStock == null)
            throw new KeyNotFoundException($"Opening stock with id {dto.Id} not found");

        ValidateOpeningStock(dto);
        dto.OpeningStockNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.OpeningStock,
            dto.OpeningStockNumber,
            dto.Id);

        _mapper.Map(dto, openingStock);
        openingStock.UpdatedAt = DateTime.UtcNow;
        openingStock.UpdatedBy = currentUserId;

        await _openingStockRepository.UpdateAsync(openingStock);
    }

    public async Task DeleteAsync(int id)
    {
        await _openingStockRepository.DeleteAsync(id);
    }

    private static void ValidateOpeningStock(CreateOpeningStockDto dto)
    {
        TransactionValidation.RequireDate(dto.Date, "Opening stock date");
        TransactionValidation.RequireDetails(dto.OpeningStockDetails, "Opening stock details");

        var lineNumber = 1;
        foreach (var detail in dto.OpeningStockDetails)
        {
            TransactionValidation.RequirePositive(detail.ItemId, $"Opening stock line {lineNumber} item");
            TransactionValidation.RequirePositive(detail.Quantity, $"Opening stock line {lineNumber} quantity");
            TransactionValidation.RequireZeroOrPositive(detail.PurchaseRate, $"Opening stock line {lineNumber} purchase rate");
            TransactionValidation.RequireZeroOrPositive(detail.CostRate, $"Opening stock line {lineNumber} cost rate");
            TransactionValidation.RequireZeroOrPositive(detail.RetailRate, $"Opening stock line {lineNumber} retail rate");
            TransactionValidation.RequireZeroOrPositive(detail.WholesaleRate, $"Opening stock line {lineNumber} wholesale rate");
            TransactionValidation.RequireZeroOrPositive(detail.MRP, $"Opening stock line {lineNumber} MRP");
            lineNumber++;
        }
    }
}
