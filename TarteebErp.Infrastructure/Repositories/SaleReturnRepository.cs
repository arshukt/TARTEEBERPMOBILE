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
}
