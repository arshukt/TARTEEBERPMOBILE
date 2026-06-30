namespace TarteebErp.Domain.Entities;

public class OpeningStock : BaseEntity
{
    public string OpeningStockNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public ICollection<OpeningStockDetail> OpeningStockDetails { get; set; } = new List<OpeningStockDetail>();
}
