namespace TarteebErp.Domain.Entities;

public class SaleReturn : BaseEntity
{
    public int SaleId { get; set; }
    public DateTime ReturnDate { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string? Notes { get; set; }
    public ICollection<SaleReturnDetail> SaleReturnDetails { get; set; } = new List<SaleReturnDetail>();
}
