using AutoMapper;
using TarteebErp.Application.DTOs;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Shared.Enums;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _itemRepository;
    private readonly IItemRepositoryWithDetails _itemRepositoryWithDetails;
    private readonly IMapper _mapper;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IStockTransactionRepository _stockTransactionRepository;

    public ItemService(
        IItemRepository itemRepository,
        IItemRepositoryWithDetails itemRepositoryWithDetails,
        IMapper mapper,
        ICategoryRepository categoryRepository,
        IBrandRepository brandRepository,
        IUnitRepository unitRepository,
        IStockTransactionRepository stockTransactionRepository)
    {
        _itemRepository = itemRepository;
        _itemRepositoryWithDetails = itemRepositoryWithDetails;
        _mapper = mapper;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _unitRepository = unitRepository;
        _stockTransactionRepository = stockTransactionRepository;
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

    public async Task<IEnumerable<ItemImportResultDto>> ImportAsync(IEnumerable<ImportItemDto> dtos, int currentUserId)
    {
        var results = new List<ItemImportResultDto>();
        var categories = (await _categoryRepository.GetAllAsync()).ToDictionary(c => c.CategoryName.Trim(), c => c.Id, StringComparer.OrdinalIgnoreCase);
        var brands = (await _brandRepository.GetAllAsync()).ToDictionary(b => b.BrandName.Trim(), b => b.Id, StringComparer.OrdinalIgnoreCase);
        var units = (await _unitRepository.GetAllAsync()).ToDictionary(u => u.UnitName.Trim(), u => u.Id, StringComparer.OrdinalIgnoreCase);

        foreach (var dto in dtos)
        {
            var result = new ItemImportResultDto
            {
                ItemCode = dto.ItemCode,
                ItemName = dto.ItemName,
            };

            try
            {
                if (!categories.TryGetValue(dto.CategoryName, out var categoryId))
                {
                    throw new ArgumentException($"Category '{dto.CategoryName}' not found");
                }

                if (!brands.TryGetValue(dto.BrandName, out var brandId))
                {
                    throw new ArgumentException($"Brand '{dto.BrandName}' not found");
                }

                if (!units.TryGetValue(dto.UnitName, out var unitId))
                {
                    throw new ArgumentException($"Unit '{dto.UnitName}' not found");
                }

                var createDto = new CreateItemDto
                {
                    Barcode = dto.Barcode,
                    ItemCode = dto.ItemCode,
                    ItemName = dto.ItemName,
                    CategoryId = categoryId,
                    BrandId = brandId,
                    UnitId = unitId,
                    PurchaseRate = dto.PurchaseRate,
                    CostRate = dto.CostRate,
                    WholesaleRate = dto.WholesaleRate,
                    RetailRate = dto.RetailRate,
                    MRP = dto.MRP,
                    TaxPercentage = dto.TaxPercentage,
                    MinimumStock = dto.MinimumStock,
                    OpeningStock = dto.OpeningStock,
                    IsActive = dto.IsActive,
                };

                var created = await CreateAsync(createDto, currentUserId);

                if (dto.OpeningStock > 0)
                {
                    await _stockTransactionRepository.AddTransactionAsync(new StockTransaction
                    {
                        ItemId = created.Id,
                        TransactionType = StockTransactionType.OpeningStock,
                        QuantityIn = dto.OpeningStock,
                        QuantityOut = 0,
                        ReferenceId = created.Id,
                        ReferenceType = "ItemImport",
                        Notes = $"Imported with opening stock {dto.OpeningStock}",
                        TransactionDate = DateTime.UtcNow
                    });
                }

                result.Success = true;
                result.ItemId = created.Id;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
            }

            results.Add(result);
        }

        return results;
    }
}
