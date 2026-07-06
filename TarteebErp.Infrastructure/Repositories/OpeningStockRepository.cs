using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class OpeningStockRepository : RepositoryBase<OpeningStock>, IOpeningStockRepository
{
    protected override string TableName => "OpeningStocks";
    protected override string[] SearchableColumns => ["OpeningStockNumber", "Notes"];
    protected override string DefaultSortColumn => "Date DESC";

    public OpeningStockRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public override async Task<OpeningStock?> GetByIdAsync(int id)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string headerSql = @"
            SELECT *
            FROM ""OpeningStocks""
            WHERE ""Id"" = @Id AND ""IsDeleted"" = false";
        var openingStock = await conn.QueryFirstOrDefaultAsync<OpeningStock>(headerSql, new { Id = id });
        if (openingStock == null)
            return null;

        const string detailsSql = @"
            SELECT *
            FROM ""OpeningStockDetails""
            WHERE ""OpeningStockId"" = @OpeningStockId AND ""IsDeleted"" = false
            ORDER BY ""Id""";
        openingStock.OpeningStockDetails = (await conn.QueryAsync<OpeningStockDetail>(
            detailsSql,
            new { OpeningStockId = id })).ToList();

        return openingStock;
    }

    public override async Task AddAsync(OpeningStock openingStock)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Insert opening stock header
            const string insertHeaderSql = @"
                INSERT INTO ""OpeningStocks"" (""OpeningStockNumber"", ""Date"", ""Notes"", ""CreatedAt"", ""CreatedBy"")
                VALUES (@OpeningStockNumber, @Date, @Notes, @CreatedAt, @CreatedBy)
                RETURNING ""Id""";
            openingStock.Id = await conn.QuerySingleAsync<int>(insertHeaderSql, openingStock, transaction);

            // Insert opening stock details
            foreach (var detail in openingStock.OpeningStockDetails)
            {
                detail.OpeningStockId = openingStock.Id;
                detail.CreatedAt = openingStock.CreatedAt;
                detail.CreatedBy = openingStock.CreatedBy;
                
                const string insertDetailSql = @"
                    INSERT INTO ""OpeningStockDetails"" (
                        ""OpeningStockId"", ""ItemId"", ""PurchaseRate"", ""CostRate"", ""RetailRate"", ""WholesaleRate"", ""MRP"",
                        ""Quantity"", ""BatchNumber"", ""ExpiryDate"", ""CreatedAt"", ""CreatedBy""
                    ) VALUES (
                        @OpeningStockId, @ItemId, @PurchaseRate, @CostRate, @RetailRate, @WholesaleRate, @MRP,
                        @Quantity, @BatchNumber, @ExpiryDate, @CreatedAt, @CreatedBy
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

    public override async Task UpdateAsync(OpeningStock openingStock)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Update opening stock header
            const string updateHeaderSql = @"
                UPDATE ""OpeningStocks"" SET
                    ""OpeningStockNumber"" = @OpeningStockNumber,
                    ""Date"" = @Date,
                    ""Notes"" = @Notes,
                    ""UpdatedAt"" = @UpdatedAt,
                    ""UpdatedBy"" = @UpdatedBy
                WHERE ""Id"" = @Id";
            await conn.ExecuteAsync(updateHeaderSql, openingStock, transaction);

            // Delete existing details (soft delete)
            const string deleteDetailsSql = @"
                UPDATE ""OpeningStockDetails"" SET
                    ""IsDeleted"" = true,
                    ""DeletedAt"" = @DeletedAt,
                    ""DeletedBy"" = @DeletedBy
                WHERE ""OpeningStockId"" = @OpeningStockId";
            await conn.ExecuteAsync(deleteDetailsSql, new {
                OpeningStockId = openingStock.Id,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = openingStock.UpdatedBy
            }, transaction);

            // Insert new details
            foreach (var detail in openingStock.OpeningStockDetails)
            {
                detail.OpeningStockId = openingStock.Id;
                detail.CreatedAt = openingStock.UpdatedAt ?? DateTime.UtcNow;
                detail.CreatedBy = openingStock.UpdatedBy ?? 0;
                
                const string insertDetailSql = @"
                    INSERT INTO ""OpeningStockDetails"" (
                        ""OpeningStockId"", ""ItemId"", ""PurchaseRate"", ""CostRate"", ""RetailRate"", ""WholesaleRate"", ""MRP"",
                        ""Quantity"", ""BatchNumber"", ""ExpiryDate"", ""CreatedAt"", ""CreatedBy""
                    ) VALUES (
                        @OpeningStockId, @ItemId, @PurchaseRate, @CostRate, @RetailRate, @WholesaleRate, @MRP,
                        @Quantity, @BatchNumber, @ExpiryDate, @CreatedAt, @CreatedBy
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
