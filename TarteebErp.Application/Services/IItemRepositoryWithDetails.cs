using TarteebErp.Application.DTOs;

namespace TarteebErp.Application.Services;

public interface IItemRepositoryWithDetails
{
    Task<IEnumerable<ItemDto>> GetPagedWithDetailsAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, bool sortDescending = false);
    Task<IEnumerable<ItemDto>> GetAllWithDetailsAsync();
    Task<ItemDto?> GetByIdWithDetailsAsync(int id);
}
