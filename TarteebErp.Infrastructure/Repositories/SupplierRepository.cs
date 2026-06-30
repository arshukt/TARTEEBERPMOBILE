using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class SupplierRepository : RepositoryBase<Supplier>, ISupplierRepository
{
    protected override string TableName => "Suppliers";
    protected override string[] SearchableColumns => ["SupplierCode", "SupplierName", "Mobile", "Email"];
    protected override string DefaultSortColumn => "SupplierName";

    public SupplierRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public async Task<Supplier?> GetByCodeAsync(string supplierCode)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"SELECT * FROM ""Suppliers"" WHERE ""SupplierCode"" = @SupplierCode AND ""IsDeleted"" = false";
        return await conn.QueryFirstOrDefaultAsync<Supplier>(sql, new { SupplierCode = supplierCode });
    }

    public override async Task AddAsync(Supplier supplier)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ""Suppliers"" (
                ""SupplierCode"", ""SupplierName"", ""Mobile"", ""Email"", ""Address"", 
                ""OpeningBalance"", ""CreatedAt"", ""CreatedBy""
            ) VALUES (
                @SupplierCode, @SupplierName, @Mobile, @Email, @Address,
                @OpeningBalance, @CreatedAt, @CreatedBy
            ) RETURNING ""Id""";
        supplier.Id = await conn.QuerySingleAsync<int>(sql, supplier);
    }

    public override async Task UpdateAsync(Supplier supplier)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ""Suppliers"" SET
                ""SupplierCode"" = @SupplierCode,
                ""SupplierName"" = @SupplierName,
                ""Mobile"" = @Mobile,
                ""Email"" = @Email,
                ""Address"" = @Address,
                ""OpeningBalance"" = @OpeningBalance,
                ""UpdatedAt"" = @UpdatedAt,
                ""UpdatedBy"" = @UpdatedBy
            WHERE ""Id"" = @Id";
        await conn.ExecuteAsync(sql, supplier);
    }
}
