using System.Text.RegularExpressions;
using Dapper;
using TarteebErp.Application.Services;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Services;

public class DocumentNumberService : IDocumentNumberService
{
    private static readonly IReadOnlyDictionary<string, DocumentNumberDefinition> Definitions =
        new Dictionary<string, DocumentNumberDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            [TransactionDocumentTypes.Purchase] = new("Purchases", "InvoiceNumber", "PUR", "Purchase invoice"),
            [TransactionDocumentTypes.Sale] = new("Sales", "InvoiceNumber", "SAL", "Sale invoice"),
            [TransactionDocumentTypes.PurchaseReturn] = new("PurchaseReturns", "ReturnNumber", "PRN", "Purchase return"),
            [TransactionDocumentTypes.SaleReturn] = new("SalesReturns", "ReturnNumber", "SRN", "Sale return"),
            [TransactionDocumentTypes.StockAdjustment] = new("StockAdjustments", "AdjustmentNumber", "ADJ", "Stock adjustment"),
            [TransactionDocumentTypes.Payment] = new("Payments", "PaymentNumber", "PAY", "Payment"),
            [TransactionDocumentTypes.StockTransfer] = new("StockTransfers", "TransferNumber", "STN", "Stock transfer"),
            [TransactionDocumentTypes.OpeningStock] = new("OpeningStocks", "OpeningStockNumber", "OS", "Opening stock")
        };

    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DocumentNumberService(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<string> GetNextNumberAsync(string documentType)
    {
        var definition = GetDefinition(documentType);
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = $@"
            SELECT ""{definition.ColumnName}""
            FROM ""{definition.TableName}""
            WHERE ""IsDeleted"" = false
              AND ""{definition.ColumnName}"" ILIKE @Pattern
            ORDER BY ""Id"" DESC
            LIMIT 200";

        var existingNumbers = await connection.QueryAsync<string>(sql, new { Pattern = $"{definition.Prefix}-%" });
        var lastSequence = existingNumbers
            .Select(number => ExtractSequence(definition.Prefix, number))
            .DefaultIfEmpty(0)
            .Max();

        return $"{definition.Prefix}-{lastSequence + 1:000000}";
    }

    public async Task<string> EnsureNumberAsync(string documentType, string? number, int? currentId = null)
    {
        var preparedNumber = string.IsNullOrWhiteSpace(number)
            ? await GetNextNumberAsync(documentType)
            : number.Trim();

        await ValidateUniqueAsync(documentType, preparedNumber, currentId);
        return preparedNumber;
    }

    public async Task ValidateUniqueAsync(string documentType, string number, int? currentId = null)
    {
        var definition = GetDefinition(documentType);

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException($"{definition.Label} number is required");

        using var connection = _dbConnectionFactory.CreateConnection();
        var currentIdFilter = currentId.HasValue ? @" AND ""Id"" <> @CurrentId" : string.Empty;
        var sql = $@"
            SELECT COUNT(1)
            FROM ""{definition.TableName}""
            WHERE ""IsDeleted"" = false
              AND LOWER(""{definition.ColumnName}"") = LOWER(@Number)
              {currentIdFilter}";

        var duplicateCount = await connection.QuerySingleAsync<int>(
            sql,
            new { Number = number.Trim(), CurrentId = currentId });

        if (duplicateCount > 0)
            throw new InvalidOperationException($"{definition.Label} number '{number.Trim()}' already exists");
    }

    private static DocumentNumberDefinition GetDefinition(string documentType)
    {
        if (Definitions.TryGetValue(documentType, out var definition))
            return definition;

        throw new ArgumentException($"Unsupported document type '{documentType}'");
    }

    private static int ExtractSequence(string prefix, string? number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return 0;

        var match = Regex.Match(number.Trim(), $"^{Regex.Escape(prefix)}-(\\d+)$", RegexOptions.IgnoreCase);
        return match.Success && int.TryParse(match.Groups[1].Value, out var sequence)
            ? sequence
            : 0;
    }

    private sealed record DocumentNumberDefinition(
        string TableName,
        string ColumnName,
        string Prefix,
        string Label);
}
