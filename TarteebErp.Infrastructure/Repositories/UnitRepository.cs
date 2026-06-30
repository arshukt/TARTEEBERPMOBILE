using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class UnitRepository : RepositoryBase<Unit>, IUnitRepository
{
    protected override string TableName => "Units";
    protected override string[] SearchableColumns => ["UnitName", "Symbol"];
    protected override string DefaultSortColumn => "UnitName";

    public UnitRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public override async Task AddAsync(Unit unit)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ""Units"" (""UnitName"", ""Symbol"", ""CreatedAt"", ""CreatedBy"")
            VALUES (@UnitName, @Symbol, @CreatedAt, @CreatedBy)
            RETURNING ""Id""";
        unit.Id = await conn.QuerySingleAsync<int>(sql, unit);
    }

    public override async Task UpdateAsync(Unit unit)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ""Units"" SET
                ""UnitName"" = @UnitName,
                ""Symbol"" = @Symbol,
                ""UpdatedAt"" = @UpdatedAt,
                ""UpdatedBy"" = @UpdatedBy
            WHERE ""Id"" = @Id";
        await conn.ExecuteAsync(sql, unit);
    }
}
