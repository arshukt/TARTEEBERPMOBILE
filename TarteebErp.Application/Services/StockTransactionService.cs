using AutoMapper;
using TarteebErp.Application.DTOs;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Shared.Enums;
using TarteebErp.Shared.Requests;
using TarteebErp.Shared.Responses;

namespace TarteebErp.Application.Services;

public interface IStockTransactionService
{
    Task<PagedResponse<StockTransactionDto>> GetPagedAsync(PagedRequest request, int? itemId = null);
    Task<PagedResponse<StockTransactionDto>> GetPagedByDateAsync(PagedRequest request, DateTime? startDate, DateTime? endDate, int? itemId = null);
}

public class StockTransactionService : IStockTransactionService
{
    private readonly IStockTransactionRepository _stockTransactionRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IMapper _mapper;

    public StockTransactionService(
        IStockTransactionRepository stockTransactionRepository,
        IItemRepository itemRepository,
        IMapper mapper)
    {
        _stockTransactionRepository = stockTransactionRepository;
        _itemRepository = itemRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<StockTransactionDto>> GetPagedAsync(PagedRequest request, int? itemId = null)
    {
        // We'll need to enhance the repository to support filtering, but for now let's just get all and map
        var transactions = await _stockTransactionRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending
        );

        var totalCount = await _stockTransactionRepository.GetTotalCountAsync(request.SearchTerm);

        var dtos = _mapper.Map<List<StockTransactionDto>>(transactions);

        // We can enhance this later to get item names, but let's keep it simple for now
        foreach (var dto in dtos)
        {
            dto.TransactionTypeName = Enum.GetName(typeof(StockTransactionType), dto.TransactionType);
        }

        return new PagedResponse<StockTransactionDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResponse<StockTransactionDto>> GetPagedByDateAsync(PagedRequest request, DateTime? startDate, DateTime? endDate, int? itemId = null)
    {
        // Placeholder for date-based filtering - can enhance repository later
        return await GetPagedAsync(request, itemId);
    }
}
