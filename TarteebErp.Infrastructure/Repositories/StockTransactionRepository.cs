using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class StockTransactionRepository : RepositoryBase<StockTransaction>, IStockTransactionRepository
{
    protected override string TableName => "StockTransactions";
    protected override string[] SearchableColumns => ["Notes", "ReferenceType"];
    protected override string DefaultSortColumn => "TransactionDate DESC";

    public StockTransactionRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public override async Task AddAsync(StockTransaction transaction)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ""StockTransactions"" (
                ""ItemId"", ""TransactionType"", ""QuantityIn"", ""QuantityOut"", ""BalanceAfter"", ""ReferenceId"",
                ""ReferenceType"", ""Notes"", ""TransactionDate"", ""CreatedAt"", ""CreatedBy""
            ) VALUES (
                @ItemId, @TransactionType, @QuantityIn, @QuantityOut, @BalanceAfter, @ReferenceId,
                @ReferenceType, @Notes, @TransactionDate, @CreatedAt, @CreatedBy
            ) RETURNING ""Id""";
        transaction.Id = await conn.QuerySingleAsync<int>(sql, transaction);
    }

    public override async Task UpdateAsync(StockTransaction transaction)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ""StockTransactions"" SET
                ""Notes"" = @Notes,
                ""UpdatedAt"" = @UpdatedAt,
                ""UpdatedBy"" = @UpdatedBy
            WHERE ""Id"" = @Id";
        await conn.ExecuteAsync(sql, transaction);
    }

    public async Task<decimal> GetCurrentStockAsync(int itemId)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT COALESCE(SUM(""QuantityIn"" - ""QuantityOut""), 0)
            FROM ""StockTransactions""
            WHERE ""ItemId"" = @ItemId AND ""IsDeleted"" = false";
        return await conn.QuerySingleOrDefaultAsync<decimal>(sql, new { ItemId = itemId });
    }

    public async Task<StockTransaction> AddTransactionAsync(StockTransaction transaction)
    {
        transaction.BalanceAfter = await GetCurrentStockAsync(transaction.ItemId) + transaction.QuantityIn - transaction.QuantityOut;
        await AddAsync(transaction);
        return transaction;
    }

    public async Task<IReadOnlyCollection<int>> DeleteForReferenceAsync(string referenceType, int referenceId, int deletedBy)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ""StockTransactions"" SET
                ""IsDeleted"" = true,
                ""DeletedAt"" = @DeletedAt,
                ""DeletedBy"" = @DeletedBy
            WHERE ""ReferenceType"" = @ReferenceType
              AND ""ReferenceId"" = @ReferenceId
              AND ""IsDeleted"" = false
            RETURNING ""ItemId""";

        var affectedItemIds = await conn.QueryAsync<int>(sql, new
        {
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = deletedBy
        });

        return affectedItemIds.Distinct().ToList();
    }

    public async Task RecalculateBalancesAsync(IEnumerable<int> itemIds)
    {
        var distinctItemIds = itemIds.Distinct().ToArray();
        if (distinctItemIds.Length == 0)
            return;

        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            const string selectSql = @"
                SELECT *
                FROM ""StockTransactions""
                WHERE ""ItemId"" = @ItemId AND ""IsDeleted"" = false
                ORDER BY ""TransactionDate"", ""Id""";

            const string updateSql = @"
                UPDATE ""StockTransactions""
                SET ""BalanceAfter"" = @BalanceAfter
                WHERE ""Id"" = @Id";

            foreach (var itemId in distinctItemIds)
            {
                var rows = (await conn.QueryAsync<StockTransaction>(
                    selectSql,
                    new { ItemId = itemId },
                    transaction)).ToList();

                decimal balance = 0;
                foreach (var row in rows)
                {
                    balance += row.QuantityIn - row.QuantityOut;
                    await conn.ExecuteAsync(updateSql, new { row.Id, BalanceAfter = balance }, transaction);
                }
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
