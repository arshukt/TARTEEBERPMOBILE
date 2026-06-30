using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class SaleRepository : RepositoryBase<Sale>, ISaleRepository
{
    protected override string TableName => "Sales";
    protected override string[] SearchableColumns => ["InvoiceNumber"];
    protected override string DefaultSortColumn => "SaleDate DESC";

    public SaleRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public override async Task AddAsync(Sale sale)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Insert sale header
            const string insertHeaderSql = @"
                INSERT INTO ""Sales"" (
                    ""CustomerId"", ""SaleDate"", ""InvoiceNumber"", ""TotalAmount"", ""Discount"", ""TaxAmount"",
                    ""NetAmount"", ""PaidAmount"", ""DueAmount"", ""DueDate"", ""IsCredit"", ""CreatedAt"", ""CreatedBy""
                ) VALUES (
                    @CustomerId, @SaleDate, @InvoiceNumber, @TotalAmount, @Discount, @TaxAmount,
                    @NetAmount, @PaidAmount, @DueAmount, @DueDate, @IsCredit, @CreatedAt, @CreatedBy
                ) RETURNING ""Id""";
            sale.Id = await conn.QuerySingleAsync<int>(insertHeaderSql, sale, transaction);

            // Insert sale details
            foreach (var detail in sale.SaleDetails)
            {
                detail.SaleId = sale.Id;
                detail.CreatedAt = sale.CreatedAt;
                detail.CreatedBy = sale.CreatedBy;
                
                const string insertDetailSql = @"
                    INSERT INTO ""SaleDetails"" (
                        ""SaleId"", ""ItemId"", ""Quantity"", ""Rate"", ""Discount"", ""TaxPercentage"", ""TaxAmount"", ""Total"",
                        ""CreatedAt"", ""CreatedBy""
                    ) VALUES (
                        @SaleId, @ItemId, @Quantity, @Rate, @Discount, @TaxPercentage, @TaxAmount, @Total,
                        @CreatedAt, @CreatedBy
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

    public override async Task UpdateAsync(Sale sale)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Update sale header
            const string updateHeaderSql = @"
                UPDATE ""Sales"" SET
                    ""CustomerId"" = @CustomerId,
                    ""SaleDate"" = @SaleDate,
                    ""InvoiceNumber"" = @InvoiceNumber,
                    ""TotalAmount"" = @TotalAmount,
                    ""Discount"" = @Discount,
                    ""TaxAmount"" = @TaxAmount,
                    ""NetAmount"" = @NetAmount,
                    ""PaidAmount"" = @PaidAmount,
                    ""DueAmount"" = @DueAmount,
                    ""DueDate"" = @DueDate,
                    ""IsCredit"" = @IsCredit,
                    ""UpdatedAt"" = @UpdatedAt,
                    ""UpdatedBy"" = @UpdatedBy
                WHERE ""Id"" = @Id";
            await conn.ExecuteAsync(updateHeaderSql, sale, transaction);

            // Delete existing details (soft delete)
            const string deleteDetailsSql = @"
                UPDATE ""SaleDetails"" SET
                    ""IsDeleted"" = true,
                    ""DeletedAt"" = @DeletedAt,
                    ""DeletedBy"" = @DeletedBy
                WHERE ""SaleId"" = @SaleId";
            await conn.ExecuteAsync(deleteDetailsSql, new {
                SaleId = sale.Id,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = sale.UpdatedBy
            }, transaction);

            // Insert new details
            foreach (var detail in sale.SaleDetails)
            {
                detail.SaleId = sale.Id;
                detail.CreatedAt = sale.UpdatedAt ?? DateTime.UtcNow;
                detail.CreatedBy = sale.UpdatedBy ?? 0;
                
                const string insertDetailSql = @"
                    INSERT INTO ""SaleDetails"" (
                        ""SaleId"", ""ItemId"", ""Quantity"", ""Rate"", ""Discount"", ""TaxPercentage"", ""TaxAmount"", ""Total"",
                        ""CreatedAt"", ""CreatedBy""
                    ) VALUES (
                        @SaleId, @ItemId, @Quantity, @Rate, @Discount, @TaxPercentage, @TaxAmount, @Total,
                        @CreatedAt, @CreatedBy
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
