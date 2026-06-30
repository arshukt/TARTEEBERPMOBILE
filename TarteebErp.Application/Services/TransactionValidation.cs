namespace TarteebErp.Application.Services;

internal static class TransactionValidation
{
    public static void RequireDate(DateTime value, string fieldName)
    {
        if (value == default)
            throw new ArgumentException($"{fieldName} is required");
    }

    public static void RequirePositive(int value, string fieldName)
    {
        if (value <= 0)
            throw new ArgumentException($"{fieldName} must be greater than zero");
    }

    public static void RequirePositive(decimal value, string fieldName)
    {
        if (value <= 0)
            throw new ArgumentException($"{fieldName} must be greater than zero");
    }

    public static void RequireZeroOrPositive(decimal value, string fieldName)
    {
        if (value < 0)
            throw new ArgumentException($"{fieldName} cannot be negative");
    }

    public static void RequireDetails<T>(IEnumerable<T>? details, string fieldName)
    {
        if (details == null || !details.Any())
            throw new ArgumentException($"{fieldName} must contain at least one item");
    }

    public static void RequireText(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{fieldName} is required");
    }

    public static void RequirePercentage(decimal value, string fieldName)
    {
        if (value < 0 || value > 100)
            throw new ArgumentException($"{fieldName} must be between 0 and 100");
    }
}
