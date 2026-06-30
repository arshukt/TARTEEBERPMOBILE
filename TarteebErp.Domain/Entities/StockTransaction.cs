using TarteebErp.Shared.Enums;

namespace TarteebErp.Domain.Entities;

public class StockTransaction : BaseEntity
{
    public int ItemId { get; set; }
    public StockTransactionType TransactionType { get; set; }
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public decimal BalanceAfter { get; set; }
    public int ReferenceId { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime TransactionDate { get; set; }
}
