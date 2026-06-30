using TarteebErp.Application.DTOs;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public interface ICompanyService
{
    Task<PagedResponse<CompanyDto>> GetPagedAsync(PagedRequest request);
    Task<CompanyDto?> GetByIdAsync(int id);
    Task<IEnumerable<CompanyDto>> GetAllAsync();
    Task<CompanyDto?> GetFirstAsync();
    Task<CompanyDto> CreateAsync(CreateCompanyDto dto, int currentUserId);
    Task UpdateAsync(UpdateCompanyDto dto, int currentUserId);
    Task DeleteAsync(int id);
}