namespace TarteebErp.Application.Services;

public interface IDocumentNumberService
{
    Task<string> GetNextNumberAsync(string documentType);
    Task<string> EnsureNumberAsync(string documentType, string? number, int? currentId = null);
    Task ValidateUniqueAsync(string documentType, string number, int? currentId = null);
}

public static class TransactionDocumentTypes
{
    public const string Purchase = "purchase";
    public const string Sale = "sale";
    public const string PurchaseReturn = "purchase-return";
    public const string SaleReturn = "sale-return";
    public const string StockAdjustment = "stock-adjustment";
    public const string Payment = "payment";
    public const string StockTransfer = "stock-transfer";
    public const string OpeningStock = "opening-stock";
}
