using System.Collections.Generic;
using System.Threading.Tasks;
using TarteebErp.Domain.Entities;

namespace TarteebErp.Domain.Repositories;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int id);
    Task<int> AddAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(int id);
    Task UpdatePermissionsAsync(int roleId, IEnumerable<string> permissions);
}
