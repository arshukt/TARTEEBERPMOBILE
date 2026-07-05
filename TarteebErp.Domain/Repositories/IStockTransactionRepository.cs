using TarteebErp.Domain.Entities;

namespace TarteebErp.Domain.Repositories;

public interface IStockTransactionRepository : IRepository<StockTransaction>
{
    Task<decimal> GetCurrentStockAsync(int itemId);
    Task<StockTransaction> AddTransactionAsync(StockTransaction transaction);
    Task<IReadOnlyCollection<int>> DeleteForReferenceAsync(string referenceType, int referenceId, int deletedBy);
    Task RecalculateBalancesAsync(IEnumerable<int> itemIds);
}
