using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
{
    protected override string TableName => "Categories";
    protected override string[] SearchableColumns => ["CategoryName", "Description"];
    protected override string DefaultSortColumn => "CategoryName";

    public CategoryRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public override async Task AddAsync(Category category)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ""Categories"" (""CategoryName"", ""Description"", ""CreatedAt"", ""CreatedBy"")
            VALUES (@CategoryName, @Description, @CreatedAt, @CreatedBy)
            RETURNING ""Id""";
        category.Id = await conn.QuerySingleAsync<int>(sql, category);
    }

    public override async Task UpdateAsync(Category category)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ""Categories"" SET
                ""CategoryName"" = @CategoryName,
                ""Description"" = @Description,
                ""UpdatedAt"" = @UpdatedAt,
                ""UpdatedBy"" = @UpdatedBy
            WHERE ""Id"" = @Id";
        await conn.ExecuteAsync(sql, category);
    }
}
