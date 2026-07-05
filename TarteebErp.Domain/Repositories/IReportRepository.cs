using TarteebErp.Domain.DTOs;

namespace TarteebErp.Domain.Repositories;

public interface IReportRepository
{
    Task<IEnumerable<CurrentStockReportItemDto>> GetCurrentStockReportAsync();
    Task<IEnumerable<SalesReportItemDto>> GetSalesReportAsync(DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<PurchasesReportItemDto>> GetPurchasesReportAsync(DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<CustomerOutstandingReportItemDto>> GetCustomerOutstandingReportAsync();
    Task<IEnumerable<SupplierOutstandingReportItemDto>> GetSupplierOutstandingReportAsync();
}
