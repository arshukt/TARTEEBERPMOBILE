using TarteebErp.Application.DTOs;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public interface ICategoryService
{
    Task<PagedResponse<CategoryDto>> GetPagedAsync(PagedRequest request);
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, int currentUserId);
    Task UpdateAsync(UpdateCategoryDto dto, int currentUserId);
    Task DeleteAsync(int id);
}
