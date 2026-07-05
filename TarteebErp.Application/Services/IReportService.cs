using TarteebErp.Domain.DTOs;

namespace TarteebErp.Application.Services;

public interface IReportService
{
    Task<IEnumerable<CurrentStockReportItemDto>> GetCurrentStockReportAsync();
    Task<IEnumerable<SalesReportItemDto>> GetSalesReportAsync(DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<PurchasesReportItemDto>> GetPurchasesReportAsync(DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<CustomerOutstandingReportItemDto>> GetCustomerOutstandingReportAsync();
    Task<IEnumerable<SupplierOutstandingReportItemDto>> GetSupplierOutstandingReportAsync();
}
