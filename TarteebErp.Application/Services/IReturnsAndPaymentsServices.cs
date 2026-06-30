using TarteebErp.Application.DTOs;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public interface IPurchaseReturnService
{
    Task<PagedResponse<PurchaseReturnDto>> GetPagedAsync(PagedRequest request);
    Task<PurchaseReturnDto?> GetByIdAsync(int id);
    Task<PurchaseReturnDto> CreateAsync(CreatePurchaseReturnDto dto, int currentUserId);
    Task UpdateAsync(UpdatePurchaseReturnDto dto, int currentUserId);
    Task DeleteAsync(int id);
}

public interface ISaleReturnService
{
    Task<PagedResponse<SaleReturnDto>> GetPagedAsync(PagedRequest request);
    Task<SaleReturnDto?> GetByIdAsync(int id);
    Task<SaleReturnDto> CreateAsync(CreateSaleReturnDto dto, int currentUserId);
    Task UpdateAsync(UpdateSaleReturnDto dto, int currentUserId);
    Task DeleteAsync(int id);
}

public interface IStockAdjustmentService
{
    Task<PagedResponse<StockAdjustmentDto>> GetPagedAsync(PagedRequest request);
    Task<StockAdjustmentDto?> GetByIdAsync(int id);
    Task<StockAdjustmentDto> CreateAsync(CreateStockAdjustmentDto dto, int currentUserId);
    Task UpdateAsync(UpdateStockAdjustmentDto dto, int currentUserId);
    Task DeleteAsync(int id);
}

public interface IStockTransferService
{
    Task<PagedResponse<StockTransferDto>> GetPagedAsync(PagedRequest request);
    Task<StockTransferDto?> GetByIdAsync(int id);
    Task<StockTransferDto> CreateAsync(CreateStockTransferDto dto, int currentUserId);
    Task UpdateAsync(UpdateStockTransferDto dto, int currentUserId);
    Task DeleteAsync(int id);
}

public interface IPaymentService
{
    Task<PagedResponse<PaymentDto>> GetPagedAsync(PagedRequest request);
    Task<PaymentDto?> GetByIdAsync(int id);
    Task<PaymentDto> CreateAsync(CreatePaymentDto dto, int currentUserId);
    Task UpdateAsync(UpdatePaymentDto dto, int currentUserId);
    Task DeleteAsync(int id);
}
