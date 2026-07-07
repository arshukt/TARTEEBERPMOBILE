using Dapper;
using System.Data;
using TarteebErp.Domain.DTOs;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public ReportRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<IEnumerable<CurrentStockReportItemDto>> GetCurrentStockReportAsync()
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                i.""Id"",
                i.""ItemCode"" AS ""Code"",
                i.""ItemName"" AS ""Name"",
                c.""CategoryName"" AS ""Category"",
                COALESCE(st.""BalanceAfter"", 0) AS ""CurrentStock"",
                u.""UnitName"" AS ""Unit""
            FROM ""Items"" i
            LEFT JOIN ""Categories"" c ON i.""CategoryId"" = c.""Id""
            LEFT JOIN ""Units"" u ON i.""UnitId"" = u.""Id""
            LEFT JOIN (
                SELECT
                    ""ItemId"",
                    SUM(""QuantityIn"" - ""QuantityOut"") AS ""BalanceAfter""
                FROM ""StockTransactions""
                WHERE ""IsDeleted"" = false
                GROUP BY ""ItemId""
            ) st ON i.""Id"" = st.""ItemId""
            WHERE i.""IsDeleted"" = false
            ORDER BY i.""ItemName""";
        return await conn.QueryAsync<CurrentStockReportItemDto>(sql);
    }

    public async Task<IEnumerable<SalesReportItemDto>> GetSalesReportAsync(DateTime? startDate, DateTime? endDate)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        var sql = @"
            SELECT 
                s.""Id"" AS ""SaleId"",
                s.""SaleDate"" AS ""Date"",
                s.""InvoiceNumber"",
                COALESCE(c.""CustomerName"", 'Cash Sale') AS ""Customer"",
                s.""NetAmount"" AS ""Total""
            FROM ""Sales"" s
            LEFT JOIN ""Customers"" c ON s.""CustomerId"" = c.""Id""
            WHERE s.""IsDeleted"" = false";

        if (startDate.HasValue)
            sql += " AND s.\"SaleDate\" >= @StartDate";
        if (endDate.HasValue)
            sql += " AND s.\"SaleDate\" <= @EndDate";

        sql += " ORDER BY s.\"SaleDate\" DESC";

        return await conn.QueryAsync<SalesReportItemDto>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<IEnumerable<PurchasesReportItemDto>> GetPurchasesReportAsync(DateTime? startDate, DateTime? endDate)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        var sql = @"
            SELECT 
                p.""Id"" AS ""PurchaseId"",
                p.""PurchaseDate"" AS ""Date"",
                p.""InvoiceNumber"",
                s.""SupplierName"" AS ""Supplier"",
                p.""NetAmount"" AS ""Total""
            FROM ""Purchases"" p
            LEFT JOIN ""Suppliers"" s ON p.""SupplierId"" = s.""Id""
            WHERE p.""IsDeleted"" = false";

        if (startDate.HasValue)
            sql += " AND p.\"PurchaseDate\" >= @StartDate";
        if (endDate.HasValue)
            sql += " AND p.\"PurchaseDate\" <= @EndDate";

        sql += " ORDER BY p.\"PurchaseDate\" DESC";

        return await conn.QueryAsync<PurchasesReportItemDto>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<IEnumerable<CustomerOutstandingReportItemDto>> GetCustomerOutstandingReportAsync()
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                c.""Id"" AS ""CustomerId"",
                c.""CustomerName"",
                COALESCE(SUM(s.""DueAmount""), 0) AS ""TotalOutstanding"",
                MAX(s.""SaleDate"") AS ""LastTransaction""
            FROM ""Customers"" c
            LEFT JOIN ""Sales"" s ON c.""Id"" = s.""CustomerId"" AND s.""IsDeleted"" = false AND s.""DueAmount"" > 0
            WHERE c.""IsDeleted"" = false
            GROUP BY c.""Id"", c.""CustomerName""
            ORDER BY c.""CustomerName""";
        return await conn.QueryAsync<CustomerOutstandingReportItemDto>(sql);
    }

    public async Task<IEnumerable<SupplierOutstandingReportItemDto>> GetSupplierOutstandingReportAsync()
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                s.""Id"" AS ""SupplierId"",
                s.""SupplierName"",
                COALESCE(SUM(p.""NetAmount""), 0) AS ""TotalOutstanding"",
                MAX(p.""PurchaseDate"") AS ""LastTransaction""
            FROM ""Suppliers"" s
            LEFT JOIN ""Purchases"" p ON s.""Id"" = p.""SupplierId"" AND p.""IsDeleted"" = false
            WHERE s.""IsDeleted"" = false
            GROUP BY s.""Id"", s.""SupplierName""
            ORDER BY s.""SupplierName""";
        return await conn.QueryAsync<SupplierOutstandingReportItemDto>(sql);
    }
}
