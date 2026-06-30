namespace TarteebErp.Domain.Entities;

public class StockAdjustmentDetail : BaseEntity
{
    public int StockAdjustmentId { get; set; }
    public int ItemId { get; set; }
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public string? Reason { get; set; }
}
