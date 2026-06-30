using AutoMapper;
using TarteebErp.Application.DTOs;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<CategoryDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _categoryRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _categoryRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<CategoryDto>
        {
            Items = _mapper.Map<List<CategoryDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var items = await _categoryRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(items);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, int currentUserId)
    {
        var category = _mapper.Map<Category>(dto);
        category.CreatedAt = DateTime.UtcNow;
        category.CreatedBy = currentUserId;

        await _categoryRepository.AddAsync(category);
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task UpdateAsync(UpdateCategoryDto dto, int currentUserId)
    {
        var category = await _categoryRepository.GetByIdAsync(dto.Id);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with id {dto.Id} not found");
        }

        _mapper.Map(dto, category);
        category.UpdatedAt = DateTime.UtcNow;
        category.UpdatedBy = currentUserId;

        await _categoryRepository.UpdateAsync(category);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with id {id} not found");
        }

        await _categoryRepository.DeleteAsync(id);
    }
}
