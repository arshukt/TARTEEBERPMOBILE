using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class SaleReturnRepository : RepositoryBase<SaleReturn>, ISaleReturnRepository
{
    protected override string TableName => "SalesReturns";
    protected override string[] SearchableColumns => ["ReturnNumber"];
    protected override string DefaultSortColumn => "ReturnDate DESC";

    public SaleReturnRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public override async Task<SaleReturn?> GetByIdAsync(int id)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string headerSql = @"
            SELECT *
            FROM ""SalesReturns""
            WHERE ""Id"" = @Id AND ""IsDeleted"" = false";
        var saleReturn = await conn.QueryFirstOrDefaultAsync<SaleReturn>(headerSql, new { Id = id });
        if (saleReturn == null)
            return null;

        const string detailsSql = @"
            SELECT *
            FROM ""SaleReturnDetails""
            WHERE ""SaleReturnId"" = @SaleReturnId AND ""IsDeleted"" = false
            ORDER BY ""Id""";
        saleReturn.SaleReturnDetails = (await conn.QueryAsync<SaleReturnDetail>(
            detailsSql,
            new { SaleReturnId = id })).ToList();

        return saleReturn;
    }

    public async Task<IReadOnlyDictionary<int, decimal>> GetReturnedQuantitiesBySaleDetailAsync(int saleId, int? excludeReturnId = null)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT
                srd.""SaleDetailId"" AS ""DetailId"",
                COALESCE(SUM(srd.""Quantity""), 0) AS ""Quantity""
            FROM ""SaleReturnDetails"" srd
            INNER JOIN ""SalesReturns"" sr ON sr.""Id"" = srd.""SaleReturnId""
            WHERE sr.""SaleId"" = @SaleId
              AND sr.""IsDeleted"" = false
              AND srd.""IsDeleted"" = false
              AND (@ExcludeReturnId IS NULL OR sr.""Id"" <> @ExcludeReturnId)
            GROUP BY srd.""SaleDetailId""";

        var rows = await conn.QueryAsync<ReturnedQuantityRow>(sql, new
        {
            SaleId = saleId,
            ExcludeReturnId = excludeReturnId
        });

        return rows.ToDictionary(row => row.DetailId, row => row.Quantity);
    }

    public override async Task AddAsync(SaleReturn saleReturn)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Insert sale return header
            const string insertHeaderSql = @"
                INSERT INTO ""SalesReturns"" (
                    ""SaleId"", ""ReturnDate"", ""ReturnNumber"", ""TotalAmount"", ""Discount"", ""TaxAmount"", ""NetAmount"", ""Notes"",
                    ""CreatedAt"", ""CreatedBy""
                ) VALUES (
                    @SaleId, @ReturnDate, @ReturnNumber, @TotalAmount, @Discount, @TaxAmount, @NetAmount, @Notes,
                    @CreatedAt, @CreatedBy
                ) RETURNING ""Id""";
            saleReturn.Id = await conn.QuerySingleAsync<int>(insertHeaderSql, saleReturn, transaction);

            // Insert sale return details
            foreach (var detail in saleReturn.SaleReturnDetails)
            {
                detail.SaleReturnId = saleReturn.Id;
                detail.CreatedAt = saleReturn.CreatedAt;
                detail.CreatedBy = saleReturn.CreatedBy;
                
                const string insertDetailSql = @"
                    INSERT INTO ""SaleReturnDetails"" (
                        ""SaleReturnId"", ""SaleDetailId"", ""ItemId"", ""Quantity"", ""Rate"", ""Discount"", ""TaxPercentage"", ""TaxAmount"", ""Total"",
                        ""Reason"", ""CreatedAt"", ""CreatedBy""
                    ) VALUES (
                        @SaleReturnId, @SaleDetailId, @ItemId, @Quantity, @Rate, @Discount, @TaxPercentage, @TaxAmount, @Total,
                        @Reason, @CreatedAt, @CreatedBy
                    ) RETURNING ""Id""";
                detail.Id = await conn.QuerySingleAsync<int>(insertDetailSql, detail, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public override async Task UpdateAsync(SaleReturn saleReturn)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Update sale return header
            const string updateHeaderSql = @"
                UPDATE ""SalesReturns"" SET
                    ""SaleId"" = @SaleId,
                    ""ReturnDate"" = @ReturnDate,
                    ""ReturnNumber"" = @ReturnNumber,
                    ""TotalAmount"" = @TotalAmount,
                    ""Discount"" = @Discount,
                    ""TaxAmount"" = @TaxAmount,
                    ""NetAmount"" = @NetAmount,
                    ""Notes"" = @Notes,
                    ""UpdatedAt"" = @UpdatedAt,
                    ""UpdatedBy"" = @UpdatedBy
                WHERE ""Id"" = @Id";
            await conn.ExecuteAsync(updateHeaderSql, saleReturn, transaction);

            // Delete existing details (soft delete)
            const string deleteDetailsSql = @"
                UPDATE ""SaleReturnDetails"" SET
                    ""IsDeleted"" = true,
                    ""DeletedAt"" = @DeletedAt,
                    ""DeletedBy"" = @DeletedBy
                WHERE ""SaleReturnId"" = @SaleReturnId";
            await conn.ExecuteAsync(deleteDetailsSql, new {
                SaleReturnId = saleReturn.Id,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = saleReturn.UpdatedBy
            }, transaction);

            // Insert new details
            foreach (var detail in saleReturn.SaleReturnDetails)
            {
                detail.SaleReturnId = saleReturn.Id;
                detail.CreatedAt = saleReturn.UpdatedAt ?? DateTime.UtcNow;
                detail.CreatedBy = saleReturn.UpdatedBy ?? 0;
                
                const string insertDetailSql = @"
                    INSERT INTO ""SaleReturnDetails"" (
                        ""SaleReturnId"", ""SaleDetailId"", ""ItemId"", ""Quantity"", ""Rate"", ""Discount"", ""TaxPercentage"", ""TaxAmount"", ""Total"",
                        ""Reason"", ""CreatedAt"", ""CreatedBy""
                    ) VALUES (
                        @SaleReturnId, @SaleDetailId, @ItemId, @Quantity, @Rate, @Discount, @TaxPercentage, @TaxAmount, @Total,
                        @Reason, @CreatedAt, @CreatedBy
                    ) RETURNING ""Id""";
                detail.Id = await conn.QuerySingleAsync<int>(insertDetailSql, detail, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public override async Task DeleteAsync(int id)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            const string deleteHeaderSql = @"
                UPDATE ""SalesReturns"" SET
                    ""IsDeleted"" = true,
                    ""DeletedAt"" = @DeletedAt
                WHERE ""Id"" = @Id AND ""IsDeleted"" = false";
            await conn.ExecuteAsync(deleteHeaderSql, new { Id = id, DeletedAt = DateTime.UtcNow }, transaction);

            const string deleteDetailsSql = @"
                UPDATE ""SaleReturnDetails"" SET
                    ""IsDeleted"" = true,
                    ""DeletedAt"" = @DeletedAt
                WHERE ""SaleReturnId"" = @SaleReturnId AND ""IsDeleted"" = false";
            await conn.ExecuteAsync(deleteDetailsSql, new { SaleReturnId = id, DeletedAt = DateTime.UtcNow }, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private sealed class ReturnedQuantityRow
    {
        public int DetailId { get; set; }
        public decimal Quantity { get; set; }
    }
}
