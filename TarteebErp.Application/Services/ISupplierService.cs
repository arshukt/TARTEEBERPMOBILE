using TarteebErp.Application.DTOs;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public interface ISupplierService
{
    Task<PagedResponse<SupplierDto>> GetPagedAsync(PagedRequest request);
    Task<SupplierDto?> GetByIdAsync(int id);
    Task<IEnumerable<SupplierDto>> GetAllAsync();
    Task<SupplierDto> CreateAsync(CreateSupplierDto dto, int currentUserId);
    Task UpdateAsync(UpdateSupplierDto dto, int currentUserId);
    Task DeleteAsync(int id);
}
