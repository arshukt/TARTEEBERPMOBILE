using AutoMapper;
using TarteebErp.Application.DTOs;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public CustomerService(ICustomerRepository customerRepository, IMapper mapper)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<CustomerDto>> GetPagedAsync(PagedRequest request)
    {
        var items = await _customerRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _customerRepository.GetTotalCountAsync(request.SearchTerm);

        return new PagedResponse<CustomerDto>
        {
            Items = _mapper.Map<List<CustomerDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return _mapper.Map<List<CustomerDto>>(customers);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto, int currentUserId)
    {
        var existingByCode = await _customerRepository.GetByCodeAsync(dto.CustomerCode);
        if (existingByCode != null)
        {
            throw new InvalidOperationException($"Customer with code {dto.CustomerCode} already exists");
        }

        var customer = _mapper.Map<Customer>(dto);
        customer.CreatedAt = DateTime.UtcNow;
        customer.CreatedBy = currentUserId;

        await _customerRepository.AddAsync(customer);
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task UpdateAsync(UpdateCustomerDto dto, int currentUserId)
    {
        var customer = await _customerRepository.GetByIdAsync(dto.Id);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with id {dto.Id} not found");
        }

        if (customer.CustomerCode != dto.CustomerCode)
        {
            var existingByCode = await _customerRepository.GetByCodeAsync(dto.CustomerCode);
            if (existingByCode != null)
            {
                throw new InvalidOperationException($"Customer with code {dto.CustomerCode} already exists");
            }
        }

        _mapper.Map(dto, customer);
        customer.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedBy = currentUserId;

        await _customerRepository.UpdateAsync(customer);
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with id {id} not found");
        }

        await _customerRepository.DeleteAsync(id);
    }
}
