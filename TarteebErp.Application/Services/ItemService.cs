using AutoMapper;
using TarteebErp.Application.DTOs;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _itemRepository;
    private readonly IItemRepositoryWithDetails _itemRepositoryWithDetails;
    private readonly IMapper _mapper;

    public ItemService(IItemRepository itemRepository, IItemRepositoryWithDetails itemRepositoryWithDetails, IMapper mapper)
    {
        _itemRepository = itemRepository;
        _itemRepositoryWithDetails = itemRepositoryWithDetails;
        _mapper = mapper;
    }

    public async Task<PagedResponse<ItemDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _itemRepositoryWithDetails.GetPagedWithDetailsAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _itemRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<ItemDto>
        {
            Items = items.ToList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<ItemDto>> GetAllAsync()
    {
        return await _itemRepositoryWithDetails.GetAllWithDetailsAsync();
    }

    public async Task<ItemDto?> GetByIdAsync(int id)
    {
        return await _itemRepositoryWithDetails.GetByIdWithDetailsAsync(id);
    }

    public async Task<ItemDto> CreateAsync(CreateItemDto dto, int currentUserId)
    {
        var item = _mapper.Map<Item>(dto);
        item.CreatedAt = DateTime.UtcNow;
        item.CreatedBy = currentUserId;

        await _itemRepository.AddAsync(item);
        return await _itemRepositoryWithDetails.GetByIdWithDetailsAsync(item.Id) ?? throw new InvalidOperationException("Failed to retrieve created item");
    }

    public async Task UpdateAsync(UpdateItemDto dto, int currentUserId)
    {
        var item = await _itemRepository.GetByIdAsync(dto.Id);
        if (item == null)
        {
            throw new KeyNotFoundException($"Item with id {dto.Id} not found");
        }

        _mapper.Map(dto, item);
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = currentUserId;

        await _itemRepository.UpdateAsync(item);
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _itemRepository.GetByIdAsync(id);
        if (item == null)
        {
            throw new KeyNotFoundException($"Item with id {id} not found");
        }

        await _itemRepository.DeleteAsync(id);
    }
}
