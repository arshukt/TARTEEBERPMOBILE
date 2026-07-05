using AutoMapper;
using TarteebErp.Application.DTOs;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Shared.Enums;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IStockTransactionRepository _stockTransactionRepository;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IMapper _mapper;

    public PurchaseService(
        IPurchaseRepository purchaseRepository,
        IStockTransactionRepository stockTransactionRepository,
        IDocumentNumberService documentNumberService,
        IMapper mapper)
    {
        _purchaseRepository = purchaseRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _documentNumberService = documentNumberService;
        _mapper = mapper;
    }

    public async Task<PagedResponse<PurchaseDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _purchaseRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _purchaseRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<PurchaseDto>
        {
            Items = _mapper.Map<List<PurchaseDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PurchaseDto?> GetByIdAsync(int id)
    {
        var purchase = await _purchaseRepository.GetByIdAsync(id);
        return _mapper.Map<PurchaseDto>(purchase);
    }

    public async Task<PurchaseDto> CreateAsync(CreatePurchaseDto dto, int currentUserId)
    {
        ValidatePurchase(dto);
        dto.InvoiceNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.Purchase,
            dto.InvoiceNumber);

        var purchase = _mapper.Map<Purchase>(dto);
        purchase.CreatedAt = DateTime.UtcNow;
        purchase.CreatedBy = currentUserId;

        CalculateTotals(purchase);

        await _purchaseRepository.AddAsync(purchase);

        // Create stock transactions for each item
        foreach (var detail in purchase.PurchaseDetails)
        {
            await _stockTransactionRepository.AddTransactionAsync(new StockTransaction
            {
                ItemId = detail.ItemId,
                TransactionType = StockTransactionType.Purchase,
                QuantityIn = detail.Quantity,
                QuantityOut = 0,
                ReferenceId = purchase.Id,
                ReferenceType = "Purchase",
                Notes = $"Purchase invoice {dto.InvoiceNumber}",
                TransactionDate = dto.PurchaseDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            });
        }

        return _mapper.Map<PurchaseDto>(purchase);
    }

    public async Task UpdateAsync(UpdatePurchaseDto dto, int currentUserId)
    {
        var purchase = await _purchaseRepository.GetByIdAsync(dto.Id);
        if (purchase == null)
            throw new KeyNotFoundException($"Purchase with id {dto.Id} not found");

        ValidatePurchase(dto);
        dto.InvoiceNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.Purchase,
            dto.InvoiceNumber,
            dto.Id);

        _mapper.Map(dto, purchase);
        CalculateTotals(purchase);
        purchase.UpdatedAt = DateTime.UtcNow;
        purchase.UpdatedBy = currentUserId;

        await _purchaseRepository.UpdateAsync(purchase);

        var affectedItemIds = new HashSet<int>(
            await _stockTransactionRepository.DeleteForReferenceAsync("Purchase", dto.Id, currentUserId));

        foreach (var detail in purchase.PurchaseDetails)
        {
            affectedItemIds.Add(detail.ItemId);
            await _stockTransactionRepository.AddTransactionAsync(new StockTransaction
            {
                ItemId = detail.ItemId,
                TransactionType = StockTransactionType.Purchase,
                QuantityIn = detail.Quantity,
                QuantityOut = 0,
                ReferenceId = purchase.Id,
                ReferenceType = "Purchase",
                Notes = $"Purchase invoice {dto.InvoiceNumber}",
                TransactionDate = dto.PurchaseDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            });
        }

        await _stockTransactionRepository.RecalculateBalancesAsync(affectedItemIds);
    }

    public async Task DeleteAsync(int id)
    {
        await _stockTransactionRepository.DeleteForReferenceAsync("Purchase", id, 0);
        await _purchaseRepository.DeleteAsync(id);
    }

    private static void CalculateTotals(Purchase purchase)
    {
        decimal totalAmount = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;

        foreach (var detail in purchase.PurchaseDetails)
        {
            detail.Total = detail.Quantity * detail.PurchaseRate;
            detail.TaxAmount = (detail.Total - detail.Discount) * (detail.TaxPercentage / 100);

            totalAmount += detail.Total;
            totalDiscount += detail.Discount;
            totalTax += detail.TaxAmount;
        }

        purchase.TotalAmount = totalAmount;
        purchase.Discount = totalDiscount;
        purchase.TaxAmount = totalTax;
        purchase.NetAmount = totalAmount - totalDiscount + totalTax;
    }

    private static void ValidatePurchase(CreatePurchaseDto dto)
    {
        TransactionValidation.RequirePositive(dto.SupplierId, "Supplier");
        TransactionValidation.RequireDate(dto.PurchaseDate, "Purchase date");
        TransactionValidation.RequireDetails(dto.PurchaseDetails, "Purchase details");

        var lineNumber = 1;
        foreach (var detail in dto.PurchaseDetails)
        {
            TransactionValidation.RequirePositive(detail.ItemId, $"Purchase line {lineNumber} item");
            TransactionValidation.RequirePositive(detail.Quantity, $"Purchase line {lineNumber} quantity");
            TransactionValidation.RequirePositive(detail.PurchaseRate, $"Purchase line {lineNumber} purchase rate");
            TransactionValidation.RequireZeroOrPositive(detail.CostRate, $"Purchase line {lineNumber} cost rate");
            TransactionValidation.RequireZeroOrPositive(detail.RetailRate, $"Purchase line {lineNumber} retail rate");
            TransactionValidation.RequireZeroOrPositive(detail.WholesaleRate, $"Purchase line {lineNumber} wholesale rate");
            TransactionValidation.RequireZeroOrPositive(detail.MRP, $"Purchase line {lineNumber} MRP");
            TransactionValidation.RequireZeroOrPositive(detail.Discount, $"Purchase line {lineNumber} discount");
            TransactionValidation.RequirePercentage(detail.TaxPercentage, $"Purchase line {lineNumber} tax percentage");

            if (detail.Discount > detail.Quantity * detail.PurchaseRate)
                throw new ArgumentException($"Purchase line {lineNumber} discount cannot be greater than line total");

            lineNumber++;
        }
    }
}
