using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class StockTransferRepository : RepositoryBase<StockTransfer>, IStockTransferRepository
{
    protected override string TableName => "StockTransfers";
    protected override string[] SearchableColumns => ["TransferNumber"];
    protected override string DefaultSortColumn => "TransferDate DESC";

    public StockTransferRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public override async Task AddAsync(StockTransfer stockTransfer)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            const string insertHeaderSql = @"
                INSERT INTO ""StockTransfers"" (""TransferDate"", ""TransferNumber"", ""FromLocation"", ""ToLocation"", ""Notes"", ""CreatedAt"", ""CreatedBy"")
                VALUES (@TransferDate, @TransferNumber, @FromLocation, @ToLocation, @Notes, @CreatedAt, @CreatedBy)
                RETURNING ""Id""";
            stockTransfer.Id = await conn.QuerySingleAsync<int>(insertHeaderSql, stockTransfer, transaction);

            foreach (var detail in stockTransfer.StockTransferDetails)
            {
                detail.StockTransferId = stockTransfer.Id;
                detail.CreatedAt = stockTransfer.CreatedAt;
                detail.CreatedBy = stockTransfer.CreatedBy;
                
                const string insertDetailSql = @"
                    INSERT INTO ""StockTransferDetails"" (""StockTransferId"", ""ItemId"", ""Quantity"", ""CreatedAt"", ""CreatedBy"")
                    VALUES (@StockTransferId, @ItemId, @Quantity, @CreatedAt, @CreatedBy)
                    RETURNING ""Id""";
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

    public override async Task UpdateAsync(StockTransfer stockTransfer)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Update stock transfer header
            const string updateHeaderSql = @"
                UPDATE ""StockTransfers"" SET
                    ""TransferDate"" = @TransferDate,
                    ""TransferNumber"" = @TransferNumber,
                    ""FromLocation"" = @FromLocation,
                    ""ToLocation"" = @ToLocation,
                    ""Notes"" = @Notes,
                    ""UpdatedAt"" = @UpdatedAt,
                    ""UpdatedBy"" = @UpdatedBy
                WHERE ""Id"" = @Id";
            await conn.ExecuteAsync(updateHeaderSql, stockTransfer, transaction);

            // Delete existing details (soft delete)
            const string deleteDetailsSql = @"
                UPDATE ""StockTransferDetails"" SET
                    ""IsDeleted"" = true,
                    ""DeletedAt"" = @DeletedAt,
                    ""DeletedBy"" = @DeletedBy
                WHERE ""StockTransferId"" = @StockTransferId";
            await conn.ExecuteAsync(deleteDetailsSql, new {
                StockTransferId = stockTransfer.Id,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = stockTransfer.UpdatedBy
            }, transaction);

            // Insert new details
            foreach (var detail in stockTransfer.StockTransferDetails)
            {
                detail.StockTransferId = stockTransfer.Id;
                detail.CreatedAt = stockTransfer.UpdatedAt ?? DateTime.UtcNow;
                detail.CreatedBy = stockTransfer.UpdatedBy ?? 0;
                
                const string insertDetailSql = @"
                    INSERT INTO ""StockTransferDetails"" (""StockTransferId"", ""ItemId"", ""Quantity"", ""CreatedAt"", ""CreatedBy"")
                    VALUES (@StockTransferId, @ItemId, @Quantity, @CreatedAt, @CreatedBy)
                    RETURNING ""Id""";
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
