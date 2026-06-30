using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
{
    protected override string TableName => "Customers";
    protected override string[] SearchableColumns => ["CustomerCode", "CustomerName", "Mobile", "Email"];
    protected override string DefaultSortColumn => "CustomerName";

    public CustomerRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public async Task<Customer?> GetByCodeAsync(string customerCode)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"SELECT * FROM ""Customers"" WHERE ""CustomerCode"" = @CustomerCode AND ""IsDeleted"" = false";
        return await conn.QueryFirstOrDefaultAsync<Customer>(sql, new { CustomerCode = customerCode });
    }

    public override async Task AddAsync(Customer customer)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ""Customers"" (
                ""CustomerCode"", ""CustomerName"", ""ContactPerson"", ""Mobile"", ""WhatsApp"", ""Email"", ""Address"", ""City"", 
                ""CreditDays"", ""CreditLimit"", ""OpeningBalance"", ""IsActive"", ""CreatedAt"", ""CreatedBy""
            ) VALUES (
                @CustomerCode, @CustomerName, @ContactPerson, @Mobile, @WhatsApp, @Email, @Address, @City,
                @CreditDays, @CreditLimit, @OpeningBalance, @IsActive, @CreatedAt, @CreatedBy
            ) RETURNING ""Id""";
        customer.Id = await conn.QuerySingleAsync<int>(sql, customer);
    }

    public override async Task UpdateAsync(Customer customer)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ""Customers"" SET
                ""CustomerCode"" = @CustomerCode,
                ""CustomerName"" = @CustomerName,
                ""ContactPerson"" = @ContactPerson,
                ""Mobile"" = @Mobile,
                ""WhatsApp"" = @WhatsApp,
                ""Email"" = @Email,
                ""Address"" = @Address,
                ""City"" = @City,
                ""CreditDays"" = @CreditDays,
                ""CreditLimit"" = @CreditLimit,
                ""OpeningBalance"" = @OpeningBalance,
                ""IsActive"" = @IsActive,
                ""UpdatedAt"" = @UpdatedAt,
                ""UpdatedBy"" = @UpdatedBy
            WHERE ""Id"" = @Id";
        await conn.ExecuteAsync(sql, customer);
    }
}
