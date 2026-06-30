using TarteebErp.Domain.Entities;

namespace TarteebErp.Domain.Repositories;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<Supplier?> GetByCodeAsync(string supplierCode);
}
