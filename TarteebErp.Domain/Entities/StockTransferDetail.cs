namespace TarteebErp.Domain.Entities;

public class StockTransferDetail : BaseEntity
{
    public int StockTransferId { get; set; }
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
}
