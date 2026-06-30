namespace TarteebErp.Domain.Entities;

public class PurchaseReturnDetail : BaseEntity
{
    public int PurchaseReturnId { get; set; }
    public int PurchaseDetailId { get; set; }
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal CostRate { get; set; }
    public decimal RetailRate { get; set; }
    public decimal WholesaleRate { get; set; }
    public decimal MRP { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string? Reason { get; set; }
}
