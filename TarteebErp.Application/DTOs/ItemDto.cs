namespace TarteebErp.Application.DTOs;

public class ItemDto
{
    public int Id { get; set; }
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
    public bool IsActive { get; set; }
    public string? ItemImage { get; set; }
    // Navigation properties for display
    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }
    public string? UnitName { get; set; }
}

public class CreateItemDto
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

public class UpdateItemDto : CreateItemDto
{
    public int Id { get; set; }
}
