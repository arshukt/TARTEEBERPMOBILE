namespace TarteebErp.Domain.Entities;

public class OpeningStockDetail : BaseEntity
{
    public int OpeningStockId { get; set; }
    public int ItemId { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal CostRate { get; set; }
    public decimal RetailRate { get; set; }
    public decimal WholesaleRate { get; set; }
    public decimal MRP { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
