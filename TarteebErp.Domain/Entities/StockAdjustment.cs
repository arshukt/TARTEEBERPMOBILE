namespace TarteebErp.Domain.Entities;

public class StockAdjustment : BaseEntity
{
    public DateTime AdjustmentDate { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public ICollection<StockAdjustmentDetail> StockAdjustmentDetails { get; set; } = new List<StockAdjustmentDetail>();
}
