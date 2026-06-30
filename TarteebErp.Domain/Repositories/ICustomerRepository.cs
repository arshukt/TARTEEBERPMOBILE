using TarteebErp.Domain.Entities;

namespace TarteebErp.Domain.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByCodeAsync(string customerCode);
}
