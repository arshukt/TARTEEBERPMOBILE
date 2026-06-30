using TarteebErp.Application.DTOs;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public interface IPurchaseService
{
    Task<PagedResponse<PurchaseDto>> GetPagedAsync(PagedRequest request);
    Task<PurchaseDto?> GetByIdAsync(int id);
    Task<PurchaseDto> CreateAsync(CreatePurchaseDto dto, int currentUserId);
    Task UpdateAsync(UpdatePurchaseDto dto, int currentUserId);
    Task DeleteAsync(int id);
}
