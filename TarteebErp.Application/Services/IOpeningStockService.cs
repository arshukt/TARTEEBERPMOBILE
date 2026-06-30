using TarteebErp.Application.DTOs;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public interface IOpeningStockService
{
    Task<PagedResponse<OpeningStockDto>> GetPagedAsync(PagedRequest request);
    Task<OpeningStockDto?> GetByIdAsync(int id);
    Task<OpeningStockDto> CreateAsync(CreateOpeningStockDto dto, int currentUserId);
    Task UpdateAsync(UpdateOpeningStockDto dto, int currentUserId);
    Task DeleteAsync(int id);
}
