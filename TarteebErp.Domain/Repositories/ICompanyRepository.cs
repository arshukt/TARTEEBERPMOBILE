using TarteebErp.Domain.Entities;

namespace TarteebErp.Domain.Repositories;

public interface ICompanyRepository : IRepository<Company>
{
    Task<Company?> GetFirstAsync();
}