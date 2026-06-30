using TarteebErp.Application.DTOs;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public interface ICustomerService
{
    Task<PagedResponse<CustomerDto>> GetPagedAsync(PagedRequest request);
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<IEnumerable<CustomerDto>> GetAllAsync();
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto, int currentUserId);
    Task UpdateAsync(UpdateCustomerDto dto, int currentUserId);
    Task DeleteAsync(int id);
}
