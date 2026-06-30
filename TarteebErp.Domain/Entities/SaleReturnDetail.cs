namespace TarteebErp.Domain.Entities;

public class SaleReturnDetail : BaseEntity
{
    public int SaleReturnId { get; set; }
    public int SaleDetailId { get; set; }
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string? Reason { get; set; }
}
