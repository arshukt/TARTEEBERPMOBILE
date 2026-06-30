using Dapper;
using System.Data;
using System.Text;
using TarteebErp.Application.DTOs;
using TarteebErp.Application.Services;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class ItemRepository : RepositoryBase<Item>, IItemRepository, IItemRepositoryWithDetails
{
    protected override string TableName => "Items";
    protected override string[] SearchableColumns => ["Barcode", "ItemCode", "ItemName"];
    protected override string DefaultSortColumn => "ItemName";

    public ItemRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public async Task<IEnumerable<ItemDto>> GetPagedWithDetailsAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, bool sortDescending = false)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        var sql = new StringBuilder(@"
            SELECT 
                i.*,
                c.""CategoryName"",
                b.""BrandName"",
                u.""UnitName""
            FROM ""Items"" i
            LEFT JOIN ""Categories"" c ON i.""CategoryId"" = c.""Id""
            LEFT JOIN ""Brands"" b ON i.""BrandId"" = b.""Id""
            LEFT JOIN ""Units"" u ON i.""UnitId"" = u.""Id""
            WHERE i.""IsDeleted"" = false");

        if (!string.IsNullOrWhiteSpace(searchTerm) && SearchableColumns.Length > 0)
        {
            var searchConditions = string.Join(" OR ", SearchableColumns.Select(col => $"i.\"{col}\" LIKE @SearchTerm"));
            sql.Append($" AND ({searchConditions})");
        }

        var orderByColumn = string.IsNullOrWhiteSpace(sortBy) ? DefaultSortColumn : sortBy;
        var orderDirection = sortDescending ? "DESC" : "ASC";
        sql.Append($" ORDER BY i.\"{orderByColumn}\" {orderDirection}");
        sql.Append(" LIMIT @PageSize OFFSET @Offset");

        var parameters = new
        {
            Offset = (pageNumber - 1) * pageSize,
            PageSize = pageSize,
            SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm}%"
        };

        return await conn.QueryAsync<ItemDto>(sql.ToString(), parameters);
    }

    public async Task<IEnumerable<ItemDto>> GetAllWithDetailsAsync()
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                i.*,
                c.""CategoryName"",
                b.""BrandName"",
                u.""UnitName""
            FROM ""Items"" i
            LEFT JOIN ""Categories"" c ON i.""CategoryId"" = c.""Id""
            LEFT JOIN ""Brands"" b ON i.""BrandId"" = b.""Id""
            LEFT JOIN ""Units"" u ON i.""UnitId"" = u.""Id""
            WHERE i.""IsDeleted"" = false
            ORDER BY i.""ItemName""";
        return await conn.QueryAsync<ItemDto>(sql);
    }

    public async Task<ItemDto?> GetByIdWithDetailsAsync(int id)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                i.*,
                c.""CategoryName"",
                b.""BrandName"",
                u.""UnitName""
            FROM ""Items"" i
            LEFT JOIN ""Categories"" c ON i.""CategoryId"" = c.""Id""
            LEFT JOIN ""Brands"" b ON i.""BrandId"" = b.""Id""
            LEFT JOIN ""Units"" u ON i.""UnitId"" = u.""Id""
            WHERE i.""Id"" = @Id AND i.""IsDeleted"" = false";
        return await conn.QueryFirstOrDefaultAsync<ItemDto>(sql, new { Id = id });
    }

    public override async Task AddAsync(Item item)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ""Items"" (
                ""Barcode"", ""ItemCode"", ""ItemName"", ""CategoryId"", ""BrandId"", ""UnitId"", ""PurchaseRate"", ""CostRate"", 
                ""WholesaleRate"", ""RetailRate"", ""MRP"", ""TaxPercentage"", ""MinimumStock"", ""OpeningStock"", 
                ""IsActive"", ""ItemImage"", ""CreatedAt"", ""CreatedBy""
            ) VALUES (
                @Barcode, @ItemCode, @ItemName, @CategoryId, @BrandId, @UnitId, @PurchaseRate, @CostRate,
                @WholesaleRate, @RetailRate, @MRP, @TaxPercentage, @MinimumStock, @OpeningStock,
                @IsActive, @ItemImage, @CreatedAt, @CreatedBy
            ) RETURNING ""Id""";
        item.Id = await conn.QuerySingleAsync<int>(sql, item);
    }

    public override async Task UpdateAsync(Item item)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ""Items"" SET
                ""Barcode"" = @Barcode,
                ""ItemCode"" = @ItemCode,
                ""ItemName"" = @ItemName,
                ""CategoryId"" = @CategoryId,
                ""BrandId"" = @BrandId,
                ""UnitId"" = @UnitId,
                ""PurchaseRate"" = @PurchaseRate,
                ""CostRate"" = @CostRate,
                ""WholesaleRate"" = @WholesaleRate,
                ""RetailRate"" = @RetailRate,
                ""MRP"" = @MRP,
                ""TaxPercentage"" = @TaxPercentage,
                ""MinimumStock"" = @MinimumStock,
                ""OpeningStock"" = @OpeningStock,
                ""IsActive"" = @IsActive,
                ""ItemImage"" = @ItemImage,
                ""UpdatedAt"" = @UpdatedAt,
                ""UpdatedBy"" = @UpdatedBy
            WHERE ""Id"" = @Id";
        await conn.ExecuteAsync(sql, item);
    }
}
