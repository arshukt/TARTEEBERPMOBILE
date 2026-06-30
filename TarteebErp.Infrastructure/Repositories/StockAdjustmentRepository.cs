using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class StockAdjustmentRepository : RepositoryBase<StockAdjustment>, IStockAdjustmentRepository
{
    protected override string TableName => "StockAdjustments";
    protected override string[] SearchableColumns => ["AdjustmentNumber"];
    protected override string DefaultSortColumn => "AdjustmentDate DESC";

    public StockAdjustmentRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public override async Task AddAsync(StockAdjustment stockAdjustment)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Insert stock adjustment header
            const string insertHeaderSql = @"
                INSERT INTO ""StockAdjustments"" (
                    ""AdjustmentDate"", ""AdjustmentNumber"", ""Notes"", ""CreatedAt"", ""CreatedBy""
                ) VALUES (
                    @AdjustmentDate, @AdjustmentNumber, @Notes, @CreatedAt, @CreatedBy
                ) RETURNING ""Id""";
            stockAdjustment.Id = await conn.QuerySingleAsync<int>(insertHeaderSql, stockAdjustment, transaction);

            // Insert stock adjustment details
            foreach (var detail in stockAdjustment.StockAdjustmentDetails)
            {
                detail.StockAdjustmentId = stockAdjustment.Id;
                detail.CreatedAt = stockAdjustment.CreatedAt;
                detail.CreatedBy = stockAdjustment.CreatedBy;
                
                const string insertDetailSql = @"
                    INSERT INTO ""StockAdjustmentDetails"" (
                        ""StockAdjustmentId"", ""ItemId"", ""QuantityIn"", ""QuantityOut"", ""Reason"", ""CreatedAt"", ""CreatedBy""
                    ) VALUES (
                        @StockAdjustmentId, @ItemId, @QuantityIn, @QuantityOut, @Reason, @CreatedAt, @CreatedBy
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

    public override async Task UpdateAsync(StockAdjustment stockAdjustment)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Update stock adjustment header
            const string updateHeaderSql = @"
                UPDATE ""StockAdjustments"" SET
                    ""AdjustmentDate"" = @AdjustmentDate,
                    ""AdjustmentNumber"" = @AdjustmentNumber,
                    ""Notes"" = @Notes,
                    ""UpdatedAt"" = @UpdatedAt,
                    ""UpdatedBy"" = @UpdatedBy
                WHERE ""Id"" = @Id";
            await conn.ExecuteAsync(updateHeaderSql, stockAdjustment, transaction);

            // Delete existing details (soft delete)
            const string deleteDetailsSql = @"
                UPDATE ""StockAdjustmentDetails"" SET
                    ""IsDeleted"" = true,
                    ""DeletedAt"" = @DeletedAt,
                    ""DeletedBy"" = @DeletedBy
                WHERE ""StockAdjustmentId"" = @StockAdjustmentId";
            await conn.ExecuteAsync(deleteDetailsSql, new {
                StockAdjustmentId = stockAdjustment.Id,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = stockAdjustment.UpdatedBy
            }, transaction);

            // Insert new details
            foreach (var detail in stockAdjustment.StockAdjustmentDetails)
            {
                detail.StockAdjustmentId = stockAdjustment.Id;
                detail.CreatedAt = stockAdjustment.UpdatedAt ?? DateTime.UtcNow;
                detail.CreatedBy = stockAdjustment.UpdatedBy ?? 0;
                
                const string insertDetailSql = @"
                    INSERT INTO ""StockAdjustmentDetails"" (
                        ""StockAdjustmentId"", ""ItemId"", ""QuantityIn"", ""QuantityOut"", ""Reason"", ""CreatedAt"", ""CreatedBy""
                    ) VALUES (
                        @StockAdjustmentId, @ItemId, @QuantityIn, @QuantityOut, @Reason, @CreatedAt, @CreatedBy
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
}
