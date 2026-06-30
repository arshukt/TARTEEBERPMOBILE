namespace TarteebErp.Domain.Entities;

public class Item : BaseEntity
{
    public string? Barcode { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public int UnitId { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal CostRate { get; set; }
    public decimal WholesaleRate { get; set; }
    public decimal RetailRate { get; set; }
    public decimal MRP { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal OpeningStock { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ItemImage { get; set; }
}
