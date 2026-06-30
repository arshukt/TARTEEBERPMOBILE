using AutoMapper;
using TarteebErp.Application.DTOs;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IMapper _mapper;

    public CompanyService(ICompanyRepository companyRepository, IMapper mapper)
    {
        _companyRepository = companyRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<CompanyDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _companyRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _companyRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<CompanyDto>
        {
            Items = _mapper.Map<List<CompanyDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<CompanyDto>> GetAllAsync()
    {
        var items = await _companyRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CompanyDto>>(items);
    }

    public async Task<CompanyDto?> GetByIdAsync(int id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        return _mapper.Map<CompanyDto>(company);
    }

    public async Task<CompanyDto?> GetFirstAsync()
    {
        var company = await _companyRepository.GetFirstAsync();
        return _mapper.Map<CompanyDto>(company);
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyDto dto, int currentUserId)
    {
        var company = _mapper.Map<Company>(dto);
        company.CreatedAt = DateTime.UtcNow;
        company.CreatedBy = currentUserId;

        await _companyRepository.AddAsync(company);
        return _mapper.Map<CompanyDto>(company);
    }

    public async Task UpdateAsync(UpdateCompanyDto dto, int currentUserId)
    {
        var company = await _companyRepository.GetByIdAsync(dto.Id);
        if (company == null)
        {
            throw new KeyNotFoundException($"Company with id {dto.Id} not found");
        }

        _mapper.Map(dto, company);
        company.UpdatedAt = DateTime.UtcNow;
        company.UpdatedBy = currentUserId;

        await _companyRepository.UpdateAsync(company);
    }

    public async Task DeleteAsync(int id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null)
        {
            throw new KeyNotFoundException($"Company with id {id} not found");
        }

        await _companyRepository.DeleteAsync(id);
    }
}