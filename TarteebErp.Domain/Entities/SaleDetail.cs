namespace TarteebErp.Domain.Entities;

public class SaleDetail : BaseEntity
{
    public int SaleId { get; set; }
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
}
