using TarteebErp.Application.DTOs;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public interface ISaleService
{
    Task<PagedResponse<SaleDto>> GetPagedAsync(PagedRequest request);
    Task<SaleDto?> GetByIdAsync(int id);
    Task<SaleDto> CreateAsync(CreateSaleDto dto, int currentUserId);
    Task UpdateAsync(UpdateSaleDto dto, int currentUserId);
    Task DeleteAsync(int id);
}
