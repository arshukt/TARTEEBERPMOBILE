using AutoMapper;
using TarteebErp.Application.DTOs;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Shared.Enums;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public class PurchaseReturnService : IPurchaseReturnService
{
    private readonly IPurchaseReturnRepository _purchaseReturnRepository;
    private readonly IStockTransactionRepository _stockTransactionRepository;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IMapper _mapper;

    public PurchaseReturnService(
        IPurchaseReturnRepository purchaseReturnRepository,
        IStockTransactionRepository stockTransactionRepository,
        IDocumentNumberService documentNumberService,
        IMapper mapper)
    {
        _purchaseReturnRepository = purchaseReturnRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _documentNumberService = documentNumberService;
        _mapper = mapper;
    }

    public async Task<PagedResponse<PurchaseReturnDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _purchaseReturnRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _purchaseReturnRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<PurchaseReturnDto>
        {
            Items = _mapper.Map<List<PurchaseReturnDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PurchaseReturnDto?> GetByIdAsync(int id)
    {
        var purchaseReturn = await _purchaseReturnRepository.GetByIdAsync(id);
        return _mapper.Map<PurchaseReturnDto>(purchaseReturn);
    }

    public async Task<PurchaseReturnDto> CreateAsync(CreatePurchaseReturnDto dto, int currentUserId)
    {
        ValidatePurchaseReturn(dto);
        dto.ReturnNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.PurchaseReturn,
            dto.ReturnNumber);

        var purchaseReturn = _mapper.Map<PurchaseReturn>(dto);
        purchaseReturn.CreatedAt = DateTime.UtcNow;
        purchaseReturn.CreatedBy = currentUserId;

        CalculateTotals(purchaseReturn);

        await _purchaseReturnRepository.AddAsync(purchaseReturn);

        // Create stock transactions for each item (stock out)
        foreach (var detail in purchaseReturn.PurchaseReturnDetails)
        {
            await _stockTransactionRepository.AddTransactionAsync(new StockTransaction
            {
                ItemId = detail.ItemId,
                TransactionType = StockTransactionType.PurchaseReturn,
                QuantityIn = 0,
                QuantityOut = detail.Quantity,
                ReferenceId = purchaseReturn.Id,
                ReferenceType = "PurchaseReturn",
                Notes = $"Purchase return {dto.ReturnNumber}",
                TransactionDate = dto.ReturnDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            });
        }

        return _mapper.Map<PurchaseReturnDto>(purchaseReturn);
    }

    public async Task UpdateAsync(UpdatePurchaseReturnDto dto, int currentUserId)
    {
        var purchaseReturn = await _purchaseReturnRepository.GetByIdAsync(dto.Id);
        if (purchaseReturn == null)
            throw new KeyNotFoundException($"PurchaseReturn with id {dto.Id} not found");

        ValidatePurchaseReturn(dto);
        dto.ReturnNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.PurchaseReturn,
            dto.ReturnNumber,
            dto.Id);

        _mapper.Map(dto, purchaseReturn);
        CalculateTotals(purchaseReturn);
        purchaseReturn.UpdatedAt = DateTime.UtcNow;
        purchaseReturn.UpdatedBy = currentUserId;

        await _purchaseReturnRepository.UpdateAsync(purchaseReturn);
    }

    public async Task DeleteAsync(int id)
    {
        await _purchaseReturnRepository.DeleteAsync(id);
    }

    private static void CalculateTotals(PurchaseReturn purchaseReturn)
    {
        decimal totalAmount = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;

        foreach (var detail in purchaseReturn.PurchaseReturnDetails)
        {
            detail.Total = detail.Quantity * detail.PurchaseRate;
            detail.TaxAmount = (detail.Total - detail.Discount) * (detail.TaxPercentage / 100);

            totalAmount += detail.Total;
            totalDiscount += detail.Discount;
            totalTax += detail.TaxAmount;
        }

        purchaseReturn.TotalAmount = totalAmount;
        purchaseReturn.Discount = totalDiscount;
        purchaseReturn.TaxAmount = totalTax;
        purchaseReturn.NetAmount = totalAmount - totalDiscount + totalTax;
    }

    private static void ValidatePurchaseReturn(CreatePurchaseReturnDto dto)
    {
        TransactionValidation.RequirePositive(dto.PurchaseId, "Purchase");
        TransactionValidation.RequireDate(dto.ReturnDate, "Purchase return date");
        TransactionValidation.RequireDetails(dto.PurchaseReturnDetails, "Purchase return details");

        var lineNumber = 1;
        foreach (var detail in dto.PurchaseReturnDetails)
        {
            TransactionValidation.RequirePositive(detail.PurchaseDetailId, $"Purchase return line {lineNumber} purchase detail");
            TransactionValidation.RequirePositive(detail.ItemId, $"Purchase return line {lineNumber} item");
            TransactionValidation.RequirePositive(detail.Quantity, $"Purchase return line {lineNumber} quantity");
            TransactionValidation.RequirePositive(detail.PurchaseRate, $"Purchase return line {lineNumber} purchase rate");
            TransactionValidation.RequireZeroOrPositive(detail.CostRate, $"Purchase return line {lineNumber} cost rate");
            TransactionValidation.RequireZeroOrPositive(detail.RetailRate, $"Purchase return line {lineNumber} retail rate");
            TransactionValidation.RequireZeroOrPositive(detail.WholesaleRate, $"Purchase return line {lineNumber} wholesale rate");
            TransactionValidation.RequireZeroOrPositive(detail.MRP, $"Purchase return line {lineNumber} MRP");
            TransactionValidation.RequireZeroOrPositive(detail.Discount, $"Purchase return line {lineNumber} discount");
            TransactionValidation.RequirePercentage(detail.TaxPercentage, $"Purchase return line {lineNumber} tax percentage");

            if (detail.Discount > detail.Quantity * detail.PurchaseRate)
                throw new ArgumentException($"Purchase return line {lineNumber} discount cannot be greater than line total");

            lineNumber++;
        }
    }
}

public class SaleReturnService : ISaleReturnService
{
    private readonly ISaleReturnRepository _saleReturnRepository;
    private readonly IStockTransactionRepository _stockTransactionRepository;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IMapper _mapper;

    public SaleReturnService(
        ISaleReturnRepository saleReturnRepository,
        IStockTransactionRepository stockTransactionRepository,
        IDocumentNumberService documentNumberService,
        IMapper mapper)
    {
        _saleReturnRepository = saleReturnRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _documentNumberService = documentNumberService;
        _mapper = mapper;
    }

    public async Task<PagedResponse<SaleReturnDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _saleReturnRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _saleReturnRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<SaleReturnDto>
        {
            Items = _mapper.Map<List<SaleReturnDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<SaleReturnDto?> GetByIdAsync(int id)
    {
        var saleReturn = await _saleReturnRepository.GetByIdAsync(id);
        return _mapper.Map<SaleReturnDto>(saleReturn);
    }

    public async Task<SaleReturnDto> CreateAsync(CreateSaleReturnDto dto, int currentUserId)
    {
        ValidateSaleReturn(dto);
        dto.ReturnNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.SaleReturn,
            dto.ReturnNumber);

        var saleReturn = _mapper.Map<SaleReturn>(dto);
        saleReturn.CreatedAt = DateTime.UtcNow;
        saleReturn.CreatedBy = currentUserId;

        CalculateTotals(saleReturn);

        await _saleReturnRepository.AddAsync(saleReturn);

        // Create stock transactions for each item (stock in)
        foreach (var detail in saleReturn.SaleReturnDetails)
        {
            await _stockTransactionRepository.AddTransactionAsync(new StockTransaction
            {
                ItemId = detail.ItemId,
                TransactionType = StockTransactionType.SaleReturn,
                QuantityIn = detail.Quantity,
                QuantityOut = 0,
                ReferenceId = saleReturn.Id,
                ReferenceType = "SaleReturn",
                Notes = $"Sale return {dto.ReturnNumber}",
                TransactionDate = dto.ReturnDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            });
        }

        return _mapper.Map<SaleReturnDto>(saleReturn);
    }

    public async Task UpdateAsync(UpdateSaleReturnDto dto, int currentUserId)
    {
        var saleReturn = await _saleReturnRepository.GetByIdAsync(dto.Id);
        if (saleReturn == null)
            throw new KeyNotFoundException($"SaleReturn with id {dto.Id} not found");

        ValidateSaleReturn(dto);
        dto.ReturnNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.SaleReturn,
            dto.ReturnNumber,
            dto.Id);

        _mapper.Map(dto, saleReturn);
        CalculateTotals(saleReturn);
        saleReturn.UpdatedAt = DateTime.UtcNow;
        saleReturn.UpdatedBy = currentUserId;

        await _saleReturnRepository.UpdateAsync(saleReturn);
    }

    public async Task DeleteAsync(int id)
    {
        await _saleReturnRepository.DeleteAsync(id);
    }

    private static void CalculateTotals(SaleReturn saleReturn)
    {
        decimal totalAmount = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;

        foreach (var detail in saleReturn.SaleReturnDetails)
        {
            detail.Total = detail.Quantity * detail.Rate;
            detail.TaxAmount = (detail.Total - detail.Discount) * (detail.TaxPercentage / 100);

            totalAmount += detail.Total;
            totalDiscount += detail.Discount;
            totalTax += detail.TaxAmount;
        }

        saleReturn.TotalAmount = totalAmount;
        saleReturn.Discount = totalDiscount;
        saleReturn.TaxAmount = totalTax;
        saleReturn.NetAmount = totalAmount - totalDiscount + totalTax;
    }

    private static void ValidateSaleReturn(CreateSaleReturnDto dto)
    {
        TransactionValidation.RequirePositive(dto.SaleId, "Sale");
        TransactionValidation.RequireDate(dto.ReturnDate, "Sale return date");
        TransactionValidation.RequireDetails(dto.SaleReturnDetails, "Sale return details");

        var lineNumber = 1;
        foreach (var detail in dto.SaleReturnDetails)
        {
            TransactionValidation.RequirePositive(detail.SaleDetailId, $"Sale return line {lineNumber} sale detail");
            TransactionValidation.RequirePositive(detail.ItemId, $"Sale return line {lineNumber} item");
            TransactionValidation.RequirePositive(detail.Quantity, $"Sale return line {lineNumber} quantity");
            TransactionValidation.RequirePositive(detail.Rate, $"Sale return line {lineNumber} rate");
            TransactionValidation.RequireZeroOrPositive(detail.Discount, $"Sale return line {lineNumber} discount");
            TransactionValidation.RequirePercentage(detail.TaxPercentage, $"Sale return line {lineNumber} tax percentage");

            if (detail.Discount > detail.Quantity * detail.Rate)
                throw new ArgumentException($"Sale return line {lineNumber} discount cannot be greater than line total");

            lineNumber++;
        }
    }
}

public class StockAdjustmentService : IStockAdjustmentService
{
    private readonly IStockAdjustmentRepository _stockAdjustmentRepository;
    private readonly IStockTransactionRepository _stockTransactionRepository;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IMapper _mapper;

    public StockAdjustmentService(
        IStockAdjustmentRepository stockAdjustmentRepository,
        IStockTransactionRepository stockTransactionRepository,
        IDocumentNumberService documentNumberService,
        IMapper mapper)
    {
        _stockAdjustmentRepository = stockAdjustmentRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _documentNumberService = documentNumberService;
        _mapper = mapper;
    }

    public async Task<PagedResponse<StockAdjustmentDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _stockAdjustmentRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _stockAdjustmentRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<StockAdjustmentDto>
        {
            Items = _mapper.Map<List<StockAdjustmentDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<StockAdjustmentDto?> GetByIdAsync(int id)
    {
        var stockAdjustment = await _stockAdjustmentRepository.GetByIdAsync(id);
        return _mapper.Map<StockAdjustmentDto>(stockAdjustment);
    }

    public async Task<StockAdjustmentDto> CreateAsync(CreateStockAdjustmentDto dto, int currentUserId)
    {
        ValidateStockAdjustment(dto);
        dto.AdjustmentNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.StockAdjustment,
            dto.AdjustmentNumber);

        var stockAdjustment = _mapper.Map<StockAdjustment>(dto);
        stockAdjustment.CreatedAt = DateTime.UtcNow;
        stockAdjustment.CreatedBy = currentUserId;

        await _stockAdjustmentRepository.AddAsync(stockAdjustment);

        // Create stock transactions for each item
        foreach (var detail in stockAdjustment.StockAdjustmentDetails)
        {
            var transactionType = detail.QuantityIn > 0 
                ? StockTransactionType.StockAdjustmentIn 
                : StockTransactionType.StockAdjustmentOut;

            await _stockTransactionRepository.AddTransactionAsync(new StockTransaction
            {
                ItemId = detail.ItemId,
                TransactionType = transactionType,
                QuantityIn = detail.QuantityIn,
                QuantityOut = detail.QuantityOut,
                ReferenceId = stockAdjustment.Id,
                ReferenceType = "StockAdjustment",
                Notes = $"Stock adjustment {dto.AdjustmentNumber}",
                TransactionDate = dto.AdjustmentDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            });
        }

        return _mapper.Map<StockAdjustmentDto>(stockAdjustment);
    }

    public async Task UpdateAsync(UpdateStockAdjustmentDto dto, int currentUserId)
    {
        var stockAdjustment = await _stockAdjustmentRepository.GetByIdAsync(dto.Id);
        if (stockAdjustment == null)
            throw new KeyNotFoundException($"StockAdjustment with id {dto.Id} not found");

        ValidateStockAdjustment(dto);
        dto.AdjustmentNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.StockAdjustment,
            dto.AdjustmentNumber,
            dto.Id);

        _mapper.Map(dto, stockAdjustment);
        stockAdjustment.UpdatedAt = DateTime.UtcNow;
        stockAdjustment.UpdatedBy = currentUserId;

        await _stockAdjustmentRepository.UpdateAsync(stockAdjustment);
    }

    public async Task DeleteAsync(int id)
    {
        await _stockAdjustmentRepository.DeleteAsync(id);
    }

    private static void ValidateStockAdjustment(CreateStockAdjustmentDto dto)
    {
        TransactionValidation.RequireDate(dto.AdjustmentDate, "Stock adjustment date");
        TransactionValidation.RequireDetails(dto.StockAdjustmentDetails, "Stock adjustment details");

        var lineNumber = 1;
        foreach (var detail in dto.StockAdjustmentDetails)
        {
            TransactionValidation.RequirePositive(detail.ItemId, $"Stock adjustment line {lineNumber} item");
            TransactionValidation.RequireZeroOrPositive(detail.QuantityIn, $"Stock adjustment line {lineNumber} quantity in");
            TransactionValidation.RequireZeroOrPositive(detail.QuantityOut, $"Stock adjustment line {lineNumber} quantity out");

            if (detail.QuantityIn == 0 && detail.QuantityOut == 0)
                throw new ArgumentException($"Stock adjustment line {lineNumber} must include quantity in or quantity out");

            if (detail.QuantityIn > 0 && detail.QuantityOut > 0)
                throw new ArgumentException($"Stock adjustment line {lineNumber} cannot include both quantity in and quantity out");

            lineNumber++;
        }
    }
}

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IMapper _mapper;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IDocumentNumberService documentNumberService,
        IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _documentNumberService = documentNumberService;
        _mapper = mapper;
    }

    public async Task<PagedResponse<PaymentDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _paymentRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _paymentRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<PaymentDto>
        {
            Items = _mapper.Map<List<PaymentDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaymentDto?> GetByIdAsync(int id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentDto dto, int currentUserId)
    {
        ValidatePayment(dto);
        dto.PaymentNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.Payment,
            dto.PaymentNumber);

        var payment = _mapper.Map<Payment>(dto);
        payment.CreatedAt = DateTime.UtcNow;
        payment.CreatedBy = currentUserId;

        await _paymentRepository.AddAsync(payment);
        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task UpdateAsync(UpdatePaymentDto dto, int currentUserId)
    {
        var payment = await _paymentRepository.GetByIdAsync(dto.Id);
        if (payment == null)
            throw new KeyNotFoundException($"Payment with id {dto.Id} not found");

        ValidatePayment(dto);
        dto.PaymentNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.Payment,
            dto.PaymentNumber,
            dto.Id);

        _mapper.Map(dto, payment);
        payment.UpdatedAt = DateTime.UtcNow;
        payment.UpdatedBy = currentUserId;

        await _paymentRepository.UpdateAsync(payment);
    }

    public async Task DeleteAsync(int id)
    {
        await _paymentRepository.DeleteAsync(id);
    }

    private static void ValidatePayment(CreatePaymentDto dto)
    {
        TransactionValidation.RequireDate(dto.PaymentDate, "Payment date");
        TransactionValidation.RequirePositive(dto.PaymentType, "Payment type");
        TransactionValidation.RequirePositive(dto.PartyType, "Party type");
        TransactionValidation.RequirePositive(dto.PartyId, "Party");
        TransactionValidation.RequirePositive(dto.Amount, "Payment amount");
    }
}

public class StockTransferService : IStockTransferService
{
    private readonly IStockTransferRepository _stockTransferRepository;
    private readonly IStockTransactionRepository _stockTransactionRepository;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IMapper _mapper;

    public StockTransferService(
        IStockTransferRepository stockTransferRepository,
        IStockTransactionRepository stockTransactionRepository,
        IDocumentNumberService documentNumberService,
        IMapper mapper)
    {
        _stockTransferRepository = stockTransferRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _documentNumberService = documentNumberService;
        _mapper = mapper;
    }

    public async Task<PagedResponse<StockTransferDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _stockTransferRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _stockTransferRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<StockTransferDto>
        {
            Items = _mapper.Map<List<StockTransferDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<StockTransferDto?> GetByIdAsync(int id)
    {
        var stockTransfer = await _stockTransferRepository.GetByIdAsync(id);
        return _mapper.Map<StockTransferDto>(stockTransfer);
    }

    public async Task<StockTransferDto> CreateAsync(CreateStockTransferDto dto, int currentUserId)
    {
        ValidateStockTransfer(dto);
        dto.TransferNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.StockTransfer,
            dto.TransferNumber);

        var stockTransfer = _mapper.Map<StockTransfer>(dto);
        stockTransfer.CreatedAt = DateTime.UtcNow;
        stockTransfer.CreatedBy = currentUserId;

        await _stockTransferRepository.AddAsync(stockTransfer);

        // Create stock transactions for each item: StockTransferOut and StockTransferIn
        foreach (var detail in stockTransfer.StockTransferDetails)
        {
            // First: transfer out
            await _stockTransactionRepository.AddTransactionAsync(new StockTransaction
            {
                ItemId = detail.ItemId,
                TransactionType = StockTransactionType.StockTransferOut,
                QuantityIn = 0,
                QuantityOut = detail.Quantity,
                ReferenceId = stockTransfer.Id,
                ReferenceType = "StockTransfer",
                Notes = $"Stock transfer {dto.TransferNumber} (Out)",
                TransactionDate = dto.TransferDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            });

            // Then: transfer in
            await _stockTransactionRepository.AddTransactionAsync(new StockTransaction
            {
                ItemId = detail.ItemId,
                TransactionType = StockTransactionType.StockTransferIn,
                QuantityIn = detail.Quantity,
                QuantityOut = 0,
                ReferenceId = stockTransfer.Id,
                ReferenceType = "StockTransfer",
                Notes = $"Stock transfer {dto.TransferNumber} (In)",
                TransactionDate = dto.TransferDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            });
        }

        return _mapper.Map<StockTransferDto>(stockTransfer);
    }

    public async Task UpdateAsync(UpdateStockTransferDto dto, int currentUserId)
    {
        var stockTransfer = await _stockTransferRepository.GetByIdAsync(dto.Id);
        if (stockTransfer == null)
            throw new KeyNotFoundException($"StockTransfer with id {dto.Id} not found");

        ValidateStockTransfer(dto);
        dto.TransferNumber = await _documentNumberService.EnsureNumberAsync(
            TransactionDocumentTypes.StockTransfer,
            dto.TransferNumber,
            dto.Id);

        _mapper.Map(dto, stockTransfer);
        stockTransfer.UpdatedAt = DateTime.UtcNow;
        stockTransfer.UpdatedBy = currentUserId;

        await _stockTransferRepository.UpdateAsync(stockTransfer);
    }

    public async Task DeleteAsync(int id)
    {
        await _stockTransferRepository.DeleteAsync(id);
    }

    private static void ValidateStockTransfer(CreateStockTransferDto dto)
    {
        TransactionValidation.RequireDate(dto.TransferDate, "Stock transfer date");
        TransactionValidation.RequireText(dto.FromLocation, "From location");
        TransactionValidation.RequireText(dto.ToLocation, "To location");
        TransactionValidation.RequireDetails(dto.StockTransferDetails, "Stock transfer details");

        if (string.Equals(dto.FromLocation?.Trim(), dto.ToLocation?.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("From location and to location cannot be the same");

        var lineNumber = 1;
        foreach (var detail in dto.StockTransferDetails)
        {
            TransactionValidation.RequirePositive(detail.ItemId, $"Stock transfer line {lineNumber} item");
            TransactionValidation.RequirePositive(detail.Quantity, $"Stock transfer line {lineNumber} quantity");
            lineNumber++;
        }
    }
}
