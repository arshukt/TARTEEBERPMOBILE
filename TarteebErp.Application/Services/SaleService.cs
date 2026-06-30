using AutoMapper;
using TarteebErp.Application.DTOs;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Shared.Enums;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IStockTransactionRepository _stockTransactionRepository;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IMapper _mapper;

    public SaleService(
        ISaleRepository saleRepository,
        IStockTransactionRepository stockTransactionRepository,
        IDocumentNumberService documentNumberService,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _documentNumberService = documentNumberService;
        _mapper = mapper;
    }

    public async Task<PagedResponse<SaleDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _saleRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _saleRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<SaleDto>
        {
            Items = _mapper.Map<List<SaleDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<SaleDto?> GetByIdAsync(int id)
    {
        var sale = await _saleRepository.GetByIdAsync(id);
        return _mapper.Map<SaleDto>(sale);
    }

    public async Task<SaleDto> CreateAsync(CreateSaleDto dto, int currentUserId)
    {
        ValidateSale(dto);
        dto.InvoiceNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.Sale,
            dto.InvoiceNumber);

        var sale = _mapper.Map<Sale>(dto);
        sale.CreatedAt = DateTime.UtcNow;
        sale.CreatedBy = currentUserId;

        CalculateTotals(sale);
        ValidateSaleTotals(sale);

        await _saleRepository.AddAsync(sale);

        // Create stock transactions for each item
        foreach (var detail in sale.SaleDetails)
        {
            await _stockTransactionRepository.AddTransactionAsync(new StockTransaction
            {
                ItemId = detail.ItemId,
                TransactionType = StockTransactionType.Sale,
                QuantityIn = 0,
                QuantityOut = detail.Quantity,
                ReferenceId = sale.Id,
                ReferenceType = "Sale",
                Notes = $"Sale invoice {dto.InvoiceNumber}",
                TransactionDate = dto.SaleDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            });
        }

        return _mapper.Map<SaleDto>(sale);
    }

    public async Task UpdateAsync(UpdateSaleDto dto, int currentUserId)
    {
        var sale = await _saleRepository.GetByIdAsync(dto.Id);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with id {dto.Id} not found");

        ValidateSale(dto);
        dto.InvoiceNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.Sale,
            dto.InvoiceNumber,
            dto.Id);

        _mapper.Map(dto, sale);
        CalculateTotals(sale);
        ValidateSaleTotals(sale);
        sale.UpdatedAt = DateTime.UtcNow;
        sale.UpdatedBy = currentUserId;

        await _saleRepository.UpdateAsync(sale);
    }

    public async Task DeleteAsync(int id)
    {
        await _saleRepository.DeleteAsync(id);
    }

    private static void CalculateTotals(Sale sale)
    {
        decimal totalAmount = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;

        foreach (var detail in sale.SaleDetails)
        {
            detail.Total = detail.Quantity * detail.Rate;
            detail.TaxAmount = (detail.Total - detail.Discount) * (detail.TaxPercentage / 100);

            totalAmount += detail.Total;
            totalDiscount += detail.Discount;
            totalTax += detail.TaxAmount;
        }

        sale.TotalAmount = totalAmount;
        sale.Discount = totalDiscount;
        sale.TaxAmount = totalTax;
        sale.NetAmount = totalAmount - totalDiscount + totalTax;
        sale.DueAmount = sale.NetAmount - sale.PaidAmount;
    }

    private static void ValidateSale(CreateSaleDto dto)
    {
        TransactionValidation.RequireDate(dto.SaleDate, "Sale date");
        TransactionValidation.RequireDetails(dto.SaleDetails, "Sale details");
        TransactionValidation.RequireZeroOrPositive(dto.PaidAmount, "Paid amount");

        if (dto.IsCredit && (!dto.CustomerId.HasValue || dto.CustomerId.Value <= 0))
            throw new ArgumentException("Customer is required for credit sale");

        if (dto.IsCredit && !dto.DueDate.HasValue)
            throw new ArgumentException("Due date is required for credit sale");

        var lineNumber = 1;
        foreach (var detail in dto.SaleDetails)
        {
            TransactionValidation.RequirePositive(detail.ItemId, $"Sale line {lineNumber} item");
            TransactionValidation.RequirePositive(detail.Quantity, $"Sale line {lineNumber} quantity");
            TransactionValidation.RequirePositive(detail.Rate, $"Sale line {lineNumber} rate");
            TransactionValidation.RequireZeroOrPositive(detail.Discount, $"Sale line {lineNumber} discount");
            TransactionValidation.RequirePercentage(detail.TaxPercentage, $"Sale line {lineNumber} tax percentage");

            if (detail.Discount > detail.Quantity * detail.Rate)
                throw new ArgumentException($"Sale line {lineNumber} discount cannot be greater than line total");

            lineNumber++;
        }
    }

    private static void ValidateSaleTotals(Sale sale)
    {
        if (sale.PaidAmount > sale.NetAmount)
            throw new ArgumentException("Paid amount cannot be greater than sale net amount");
    }
}
