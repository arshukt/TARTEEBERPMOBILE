using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class PaymentRepository : RepositoryBase<Payment>, IPaymentRepository
{
    protected override string TableName => "Payments";
    protected override string[] SearchableColumns => ["PaymentNumber", "ReferenceNumber"];
    protected override string DefaultSortColumn => "PaymentDate DESC";

    public PaymentRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public override async Task AddAsync(Payment payment)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ""Payments"" (
                ""PaymentDate"", ""PaymentNumber"", ""PaymentType"", ""PartyType"", ""PartyId"", ""Amount"", ""PaymentMethod"", ""ReferenceNumber"", ""Notes"",
                ""CreatedAt"", ""CreatedBy""
            ) VALUES (
                @PaymentDate, @PaymentNumber, @PaymentType, @PartyType, @PartyId, @Amount, @PaymentMethod, @ReferenceNumber, @Notes,
                @CreatedAt, @CreatedBy
            ) RETURNING ""Id""";
        payment.Id = await conn.QuerySingleAsync<int>(sql, payment);
    }

    public override async Task UpdateAsync(Payment payment)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ""Payments"" SET
                ""PaymentDate"" = @PaymentDate,
                ""PaymentNumber"" = @PaymentNumber,
                ""PaymentType"" = @PaymentType,
                ""PartyType"" = @PartyType,
                ""PartyId"" = @PartyId,
                ""Amount"" = @Amount,
                ""PaymentMethod"" = @PaymentMethod,
                ""ReferenceNumber"" = @ReferenceNumber,
                ""Notes"" = @Notes,
                ""UpdatedAt"" = @UpdatedAt,
                ""UpdatedBy"" = @UpdatedBy
            WHERE ""Id"" = @Id";
        await conn.ExecuteAsync(sql, payment);
    }
}
