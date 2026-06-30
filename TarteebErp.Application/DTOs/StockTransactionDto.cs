namespace TarteebErp.Application.DTOs;

public class StockTransactionDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string? ItemName { get; set; }
    public int TransactionType { get; set; }
    public string? TransactionTypeName { get; set; }
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public decimal BalanceAfter { get; set; }
    public int ReferenceId { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime TransactionDate { get; set; }
}
