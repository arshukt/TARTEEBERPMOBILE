using AutoMapper;
using TarteebErp.Application.DTOs;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly IMapper _mapper;

    public BrandService(IBrandRepository brandRepository, IMapper mapper)
    {
        _brandRepository = brandRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<BrandDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _brandRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _brandRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<BrandDto>
        {
            Items = _mapper.Map<List<BrandDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<BrandDto>> GetAllAsync()
    {
        var items = await _brandRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<BrandDto>>(items);
    }

    public async Task<BrandDto?> GetByIdAsync(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);
        return _mapper.Map<BrandDto>(brand);
    }

    public async Task<BrandDto> CreateAsync(CreateBrandDto dto, int currentUserId)
    {
        var brand = _mapper.Map<Brand>(dto);
        brand.CreatedAt = DateTime.UtcNow;
        brand.CreatedBy = currentUserId;

        await _brandRepository.AddAsync(brand);
        return _mapper.Map<BrandDto>(brand);
    }

    public async Task UpdateAsync(UpdateBrandDto dto, int currentUserId)
    {
        var brand = await _brandRepository.GetByIdAsync(dto.Id);
        if (brand == null)
        {
            throw new KeyNotFoundException($"Brand with id {dto.Id} not found");
        }

        _mapper.Map(dto, brand);
        brand.UpdatedAt = DateTime.UtcNow;
        brand.UpdatedBy = currentUserId;

        await _brandRepository.UpdateAsync(brand);
    }

    public async Task DeleteAsync(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);
        if (brand == null)
        {
            throw new KeyNotFoundException($"Brand with id {id} not found");
        }

        await _brandRepository.DeleteAsync(id);
    }
}
