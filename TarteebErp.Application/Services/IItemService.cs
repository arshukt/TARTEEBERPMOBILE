using TarteebErp.Application.DTOs;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public interface IItemService
{
    Task<PagedResponse<ItemDto>> GetPagedAsync(PagedRequest request);
    Task<ItemDto?> GetByIdAsync(int id);
    Task<IEnumerable<ItemDto>> GetAllAsync();
    Task<ItemDto> CreateAsync(CreateItemDto dto, int currentUserId);
    Task UpdateAsync(UpdateItemDto dto, int currentUserId);
    Task DeleteAsync(int id);
    Task<IEnumerable<ItemImportResultDto>> ImportAsync(IEnumerable<ImportItemDto> dtos, int currentUserId);
}

public class ItemImportResultDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int? ItemId { get; set; }
}
