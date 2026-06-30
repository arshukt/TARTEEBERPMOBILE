using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class BrandRepository : RepositoryBase<Brand>, IBrandRepository
{
    protected override string TableName => "Brands";
    protected override string[] SearchableColumns => ["BrandName"];
    protected override string DefaultSortColumn => "BrandName";

    public BrandRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public override async Task AddAsync(Brand brand)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ""Brands"" (""BrandName"", ""CreatedAt"", ""CreatedBy"")
            VALUES (@BrandName, @CreatedAt, @CreatedBy)
            RETURNING ""Id""";
        brand.Id = await conn.QuerySingleAsync<int>(sql, brand);
    }

    public override async Task UpdateAsync(Brand brand)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ""Brands"" SET
                ""BrandName"" = @BrandName,
                ""UpdatedAt"" = @UpdatedAt,
                ""UpdatedBy"" = @UpdatedBy
            WHERE ""Id"" = @Id";
        await conn.ExecuteAsync(sql, brand);
    }
}
