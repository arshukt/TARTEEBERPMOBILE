using TarteebErp.Application.DTOs;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public interface IBrandService
{
    Task<PagedResponse<BrandDto>> GetPagedAsync(PagedRequest request);
    Task<BrandDto?> GetByIdAsync(int id);
    Task<IEnumerable<BrandDto>> GetAllAsync();
    Task<BrandDto> CreateAsync(CreateBrandDto dto, int currentUserId);
    Task UpdateAsync(UpdateBrandDto dto, int currentUserId);
    Task DeleteAsync(int id);
}
