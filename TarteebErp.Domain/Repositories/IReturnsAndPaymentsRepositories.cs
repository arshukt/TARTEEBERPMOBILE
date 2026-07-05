using TarteebErp.Domain.Entities;

namespace TarteebErp.Domain.Repositories;

public interface IPurchaseReturnRepository : IRepository<PurchaseReturn>
{
    Task<IReadOnlyDictionary<int, decimal>> GetReturnedQuantitiesByPurchaseDetailAsync(int purchaseId, int? excludeReturnId = null);
}

public interface ISaleReturnRepository : IRepository<SaleReturn>
{
    Task<IReadOnlyDictionary<int, decimal>> GetReturnedQuantitiesBySaleDetailAsync(int saleId, int? excludeReturnId = null);
}

public interface IStockAdjustmentRepository : IRepository<StockAdjustment>
{
}

public interface IStockTransferRepository : IRepository<StockTransfer>
{
}

public interface IPaymentRepository : IRepository<Payment>
{
}
