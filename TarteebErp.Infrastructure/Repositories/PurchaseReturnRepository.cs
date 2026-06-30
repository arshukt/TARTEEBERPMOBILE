using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class PurchaseReturnRepository : RepositoryBase<PurchaseReturn>, IPurchaseReturnRepository
{
    protected override string TableName => "PurchaseReturns";
    protected override string[] SearchableColumns => ["ReturnNumber"];
    protected override string DefaultSortColumn => "ReturnDate DESC";

    public PurchaseReturnRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public override async Task AddAsync(PurchaseReturn purchaseReturn)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Insert purchase return header
            const string insertHeaderSql = @"
                INSERT INTO ""PurchaseReturns"" (
                    ""PurchaseId"", ""ReturnDate"", ""ReturnNumber"", ""TotalAmount"", ""Discount"", ""TaxAmount"", ""NetAmount"", ""Notes"",
                    ""CreatedAt"", ""CreatedBy""
                ) VALUES (
                    @PurchaseId, @ReturnDate, @ReturnNumber, @TotalAmount, @Discount, @TaxAmount, @NetAmount, @Notes,
                    @CreatedAt, @CreatedBy
                ) RETURNING ""Id""";
            purchaseReturn.Id = await conn.QuerySingleAsync<int>(insertHeaderSql, purchaseReturn, transaction);

            // Insert purchase return details
            foreach (var detail in purchaseReturn.PurchaseReturnDetails)
            {
                detail.PurchaseReturnId = purchaseReturn.Id;
                detail.CreatedAt = purchaseReturn.CreatedAt;
                detail.CreatedBy = purchaseReturn.CreatedBy;
                
                const string insertDetailSql = @"
                    INSERT INTO ""PurchaseReturnDetails"" (
                        ""PurchaseReturnId"", ""PurchaseDetailId"", ""ItemId"", ""Quantity"", ""PurchaseRate"", ""CostRate"", ""RetailRate"", ""WholesaleRate"", ""MRP"",
                        ""Discount"", ""TaxPercentage"", ""TaxAmount"", ""Total"", ""Reason"", ""CreatedAt"", ""CreatedBy""
                    ) VALUES (
                        @PurchaseReturnId, @PurchaseDetailId, @ItemId, @Quantity, @PurchaseRate, @CostRate, @RetailRate, @WholesaleRate, @MRP,
                        @Discount, @TaxPercentage, @TaxAmount, @Total, @Reason, @CreatedAt, @CreatedBy
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

    public override async Task UpdateAsync(PurchaseReturn purchaseReturn)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Update purchase return header
            const string updateHeaderSql = @"
                UPDATE ""PurchaseReturns"" SET
                    ""PurchaseId"" = @PurchaseId,
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
            await conn.ExecuteAsync(updateHeaderSql, purchaseReturn, transaction);

            // Delete existing details (soft delete)
            const string deleteDetailsSql = @"
                UPDATE ""PurchaseReturnDetails"" SET
                    ""IsDeleted"" = true,
                    ""DeletedAt"" = @DeletedAt,
                    ""DeletedBy"" = @DeletedBy
                WHERE ""PurchaseReturnId"" = @PurchaseReturnId";
            await conn.ExecuteAsync(deleteDetailsSql, new {
                PurchaseReturnId = purchaseReturn.Id,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = purchaseReturn.UpdatedBy
            }, transaction);

            // Insert new details
            foreach (var detail in purchaseReturn.PurchaseReturnDetails)
            {
                detail.PurchaseReturnId = purchaseReturn.Id;
                detail.CreatedAt = purchaseReturn.UpdatedAt ?? DateTime.UtcNow;
                detail.CreatedBy = purchaseReturn.UpdatedBy ?? 0;
                
                const string insertDetailSql = @"
                    INSERT INTO ""PurchaseReturnDetails"" (
                        ""PurchaseReturnId"", ""PurchaseDetailId"", ""ItemId"", ""Quantity"", ""PurchaseRate"", ""CostRate"", ""RetailRate"", ""WholesaleRate"", ""MRP"",
                        ""Discount"", ""TaxPercentage"", ""TaxAmount"", ""Total"", ""Reason"", ""CreatedAt"", ""CreatedBy""
                    ) VALUES (
                        @PurchaseReturnId, @PurchaseDetailId, @ItemId, @Quantity, @PurchaseRate, @CostRate, @RetailRate, @WholesaleRate, @MRP,
                        @Discount, @TaxPercentage, @TaxAmount, @Total, @Reason, @CreatedAt, @CreatedBy
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
