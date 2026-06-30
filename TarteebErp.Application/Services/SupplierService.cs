using AutoMapper;
using TarteebErp.Application.DTOs;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;

    public SupplierService(ISupplierRepository supplierRepository, IMapper mapper)
    {
        _supplierRepository = supplierRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<SupplierDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _supplierRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _supplierRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<SupplierDto>
        {
            Items = _mapper.Map<List<SupplierDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<IEnumerable<SupplierDto>> GetAllAsync()
    {
        var suppliers = await _supplierRepository.GetAllAsync();
        return _mapper.Map<List<SupplierDto>>(suppliers);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto, int currentUserId)
    {
        var existingByCode = await _supplierRepository.GetByCodeAsync(dto.SupplierCode);
        if (existingByCode != null)
        {
            throw new InvalidOperationException($"Supplier with code {dto.SupplierCode} already exists");
        }

        var supplier = _mapper.Map<Supplier>(dto);
        supplier.CreatedAt = DateTime.UtcNow;
        supplier.CreatedBy = currentUserId;

        await _supplierRepository.AddAsync(supplier);
        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task UpdateAsync(UpdateSupplierDto dto, int currentUserId)
    {
        var supplier = await _supplierRepository.GetByIdAsync(dto.Id);
        if (supplier == null)
        {
            throw new KeyNotFoundException($"Supplier with id {dto.Id} not found");
        }

        if (supplier.SupplierCode != dto.SupplierCode)
        {
            var existingByCode = await _supplierRepository.GetByCodeAsync(dto.SupplierCode);
            if (existingByCode != null)
            {
                throw new InvalidOperationException($"Supplier with code {dto.SupplierCode} already exists");
            }
        }

        _mapper.Map(dto, supplier);
        supplier.UpdatedAt = DateTime.UtcNow;
        supplier.UpdatedBy = currentUserId;

        await _supplierRepository.UpdateAsync(supplier);
    }

    public async Task DeleteAsync(int id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
        {
            throw new KeyNotFoundException($"Supplier with id {id} not found");
        }

        await _supplierRepository.DeleteAsync(id);
    }
}
