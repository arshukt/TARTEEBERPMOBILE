using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
{
    protected override string TableName => "Companies";
    protected override string[] SearchableColumns => ["CompanyName"];
    protected override string DefaultSortColumn => "CompanyName";

    public CompanyRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public override async Task AddAsync(Company company)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ""Companies"" (""CompanyName"", ""Address"", ""Mobile"", ""Phone"", ""Email"", ""Website"", ""Logo"", ""TaxNumber"", ""CreatedAt"", ""CreatedBy"")
            VALUES (@CompanyName, @Address, @Mobile, @Phone, @Email, @Website, @Logo, @TaxNumber, @CreatedAt, @CreatedBy)
            RETURNING ""Id""";
        company.Id = await conn.QuerySingleAsync<int>(sql, company);
    }

    public override async Task UpdateAsync(Company company)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ""Companies"" SET
                ""CompanyName"" = @CompanyName,
                ""Address"" = @Address,
                ""Mobile"" = @Mobile,
                ""Phone"" = @Phone,
                ""Email"" = @Email,
                ""Website"" = @Website,
                ""Logo"" = @Logo,
                ""TaxNumber"" = @TaxNumber,
                ""UpdatedAt"" = @UpdatedAt,
                ""UpdatedBy"" = @UpdatedBy
            WHERE ""Id"" = @Id";
        await conn.ExecuteAsync(sql, company);
    }

    public async Task<Company?> GetFirstAsync()
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = "SELECT * FROM \"Companies\" WHERE \"IsDeleted\" = false LIMIT 1";
        return await conn.QuerySingleOrDefaultAsync<Company>(sql);
    }
}