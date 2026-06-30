using TarteebErp.Application.DTOs;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public interface IUnitService
{
    Task<PagedResponse<UnitDto>> GetPagedAsync(PagedRequest request);
    Task<UnitDto?> GetByIdAsync(int id);
    Task<IEnumerable<UnitDto>> GetAllAsync();
    Task<UnitDto> CreateAsync(CreateUnitDto dto, int currentUserId);
    Task UpdateAsync(UpdateUnitDto dto, int currentUserId);
    Task DeleteAsync(int id);
}
