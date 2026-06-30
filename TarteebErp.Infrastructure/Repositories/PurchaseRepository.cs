using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class PurchaseRepository : RepositoryBase<Purchase>, IPurchaseRepository
{
    protected override string TableName => "Purchases";
    protected override string[] SearchableColumns => ["InvoiceNumber"];
    protected override string DefaultSortColumn => "PurchaseDate DESC";

    public PurchaseRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public override async Task AddAsync(Purchase purchase)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Insert purchase header
            const string insertHeaderSql = @"
                INSERT INTO ""Purchases"" (
                    ""SupplierId"", ""PurchaseDate"", ""InvoiceNumber"", ""TotalAmount"", ""Discount"", ""TaxAmount"", ""NetAmount"",
                    ""CreatedAt"", ""CreatedBy""
                ) VALUES (
                    @SupplierId, @PurchaseDate, @InvoiceNumber, @TotalAmount, @Discount, @TaxAmount, @NetAmount,
                    @CreatedAt, @CreatedBy
                ) RETURNING ""Id""";
            purchase.Id = await conn.QuerySingleAsync<int>(insertHeaderSql, purchase, transaction);

            // Insert purchase details
            foreach (var detail in purchase.PurchaseDetails)
            {
                detail.PurchaseId = purchase.Id;
                detail.CreatedAt = purchase.CreatedAt;
                detail.CreatedBy = purchase.CreatedBy;
                
                const string insertDetailSql = @"
                    INSERT INTO ""PurchaseDetails"" (
                        ""PurchaseId"", ""ItemId"", ""Quantity"", ""PurchaseRate"", ""CostRate"", ""RetailRate"", ""WholesaleRate"", ""MRP"",
                        ""Discount"", ""TaxPercentage"", ""TaxAmount"", ""Total"", ""BatchNumber"", ""ExpiryDate"", ""CreatedAt"", ""CreatedBy""
                    ) VALUES (
                        @PurchaseId, @ItemId, @Quantity, @PurchaseRate, @CostRate, @RetailRate, @WholesaleRate, @MRP,
                        @Discount, @TaxPercentage, @TaxAmount, @Total, @BatchNumber, @ExpiryDate, @CreatedAt, @CreatedBy
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

    public override async Task UpdateAsync(Purchase purchase)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Update purchase header
            const string updateHeaderSql = @"
                UPDATE ""Purchases"" SET
                    ""SupplierId"" = @SupplierId,
                    ""PurchaseDate"" = @PurchaseDate,
                    ""InvoiceNumber"" = @InvoiceNumber,
                    ""TotalAmount"" = @TotalAmount,
                    ""Discount"" = @Discount,
                    ""TaxAmount"" = @TaxAmount,
                    ""NetAmount"" = @NetAmount,
                    ""UpdatedAt"" = @UpdatedAt,
                    ""UpdatedBy"" = @UpdatedBy
                WHERE ""Id"" = @Id";
            await conn.ExecuteAsync(updateHeaderSql, purchase, transaction);

            // Delete existing details (soft delete)
            const string deleteDetailsSql = @"
                UPDATE ""PurchaseDetails"" SET
                    ""IsDeleted"" = true,
                    ""DeletedAt"" = @DeletedAt,
                    ""DeletedBy"" = @DeletedBy
                WHERE ""PurchaseId"" = @PurchaseId";
            await conn.ExecuteAsync(deleteDetailsSql, new {
                PurchaseId = purchase.Id,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = purchase.UpdatedBy
            }, transaction);

            // Insert new details
            foreach (var detail in purchase.PurchaseDetails)
            {
                detail.PurchaseId = purchase.Id;
                detail.CreatedAt = purchase.UpdatedAt ?? DateTime.UtcNow;
                detail.CreatedBy = purchase.UpdatedBy ?? 0;
                
                const string insertDetailSql = @"
                    INSERT INTO ""PurchaseDetails"" (
                        ""PurchaseId"", ""ItemId"", ""Quantity"", ""PurchaseRate"", ""CostRate"", ""RetailRate"", ""WholesaleRate"", ""MRP"",
                        ""Discount"", ""TaxPercentage"", ""TaxAmount"", ""Total"", ""BatchNumber"", ""ExpiryDate"", ""CreatedAt"", ""CreatedBy""
                    ) VALUES (
                        @PurchaseId, @ItemId, @Quantity, @PurchaseRate, @CostRate, @RetailRate, @WholesaleRate, @MRP,
                        @Discount, @TaxPercentage, @TaxAmount, @Total, @BatchNumber, @ExpiryDate, @CreatedAt, @CreatedBy
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
