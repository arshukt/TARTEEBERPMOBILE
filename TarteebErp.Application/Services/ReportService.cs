using TarteebErp.Domain.DTOs;
using TarteebErp.Domain.Repositories;

namespace TarteebErp.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<IEnumerable<CurrentStockReportItemDto>> GetCurrentStockReportAsync()
    {
        return await _reportRepository.GetCurrentStockReportAsync();
    }

    public async Task<IEnumerable<SalesReportItemDto>> GetSalesReportAsync(DateTime? startDate, DateTime? endDate)
    {
        return await _reportRepository.GetSalesReportAsync(startDate, endDate);
    }

    public async Task<IEnumerable<PurchasesReportItemDto>> GetPurchasesReportAsync(DateTime? startDate, DateTime? endDate)
    {
        return await _reportRepository.GetPurchasesReportAsync(startDate, endDate);
    }

    public async Task<IEnumerable<CustomerOutstandingReportItemDto>> GetCustomerOutstandingReportAsync()
    {
        return await _reportRepository.GetCustomerOutstandingReportAsync();
    }

    public async Task<IEnumerable<SupplierOutstandingReportItemDto>> GetSupplierOutstandingReportAsync()
    {
        return await _reportRepository.GetSupplierOutstandingReportAsync();
    }
}
