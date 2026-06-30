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
            SELECT COALESCE(""BalanceAfter"", 0)
            FROM ""StockTransactions""
            WHERE ""ItemId"" = @ItemId AND ""IsDeleted"" = false
            ORDER BY ""TransactionDate"" DESC, ""Id"" DESC
            LIMIT 1";
        return await conn.QuerySingleOrDefaultAsync<decimal>(sql, new { ItemId = itemId });
    }

    public async Task<StockTransaction> AddTransactionAsync(StockTransaction transaction)
    {
        transaction.BalanceAfter = await GetCurrentStockAsync(transaction.ItemId) + transaction.QuantityIn - transaction.QuantityOut;
        await AddAsync(transaction);
        return transaction;
    }
}
