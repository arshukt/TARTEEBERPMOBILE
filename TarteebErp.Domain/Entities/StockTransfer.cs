namespace TarteebErp.Domain.Entities;

public class StockTransfer : BaseEntity
{
    public DateTime TransferDate { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public string? FromLocation { get; set; }
    public string? ToLocation { get; set; }
    public string? Notes { get; set; }
    public ICollection<StockTransferDetail> StockTransferDetails { get; set; } = new List<StockTransferDetail>();
}
