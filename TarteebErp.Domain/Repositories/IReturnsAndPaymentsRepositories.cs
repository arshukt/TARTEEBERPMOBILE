using TarteebErp.Domain.Entities;

namespace TarteebErp.Domain.Repositories;

public interface IPurchaseReturnRepository : IRepository<PurchaseReturn>
{
}

public interface ISaleReturnRepository : IRepository<SaleReturn>
{
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
