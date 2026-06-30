using AutoMapper;
using TarteebErp.Application.DTOs;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public class UnitService : IUnitService
{
    private readonly IUnitRepository _unitRepository;
    private readonly IMapper _mapper;

    public UnitService(IUnitRepository unitRepository, IMapper mapper)
    {
        _unitRepository = unitRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<UnitDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _unitRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _unitRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<UnitDto>
        {
            Items = _mapper.Map<List<UnitDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<UnitDto>> GetAllAsync()
    {
        var items = await _unitRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UnitDto>>(items);
    }

    public async Task<UnitDto?> GetByIdAsync(int id)
    {
        var unit = await _unitRepository.GetByIdAsync(id);
        return _mapper.Map<UnitDto>(unit);
    }

    public async Task<UnitDto> CreateAsync(CreateUnitDto dto, int currentUserId)
    {
        var unit = _mapper.Map<Unit>(dto);
        unit.CreatedAt = DateTime.UtcNow;
        unit.CreatedBy = currentUserId;

        await _unitRepository.AddAsync(unit);
        return _mapper.Map<UnitDto>(unit);
    }

    public async Task UpdateAsync(UpdateUnitDto dto, int currentUserId)
    {
        var unit = await _unitRepository.GetByIdAsync(dto.Id);
        if (unit == null)
        {
            throw new KeyNotFoundException($"Unit with id {dto.Id} not found");
        }

        _mapper.Map(dto, unit);
        unit.UpdatedAt = DateTime.UtcNow;
        unit.UpdatedBy = currentUserId;

        await _unitRepository.UpdateAsync(unit);
    }

    public async Task DeleteAsync(int id)
    {
        var unit = await _unitRepository.GetByIdAsync(id);
        if (unit == null)
        {
            throw new KeyNotFoundException($"Unit with id {id} not found");
        }

        await _unitRepository.DeleteAsync(id);
    }
}
