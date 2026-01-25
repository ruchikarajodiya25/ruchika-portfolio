namespace ServiceHubPro.Application.Common.Helpers;

/// <summary>
/// Helper class for tax rate calculations.
/// Tax rates are stored as percent (0-100), e.g., 8 for 8%.
/// </summary>
public static class TaxRateHelper
{
    /// <summary>
    /// Converts tax rate from percent (0-100) to decimal fraction (0-1).
    /// Example: 8 (percent) -> 0.08 (decimal fraction)
    /// </summary>
    public static decimal GetDecimalFraction(decimal taxRatePercent)
    {
        return taxRatePercent / 100m;
    }

    /// <summary>
    /// Calculates the total amount for an item including tax.
    /// Formula: (quantity * unitPrice) * (1 + taxRate/100)
    /// </summary>
    public static decimal CalculateItemTotal(decimal quantity, decimal unitPrice, decimal taxRatePercent)
    {
        var taxDecimal = GetDecimalFraction(taxRatePercent);
        return quantity * unitPrice * (1m + taxDecimal);
    }

    /// <summary>
    /// Calculates the tax amount for an item.
    /// Formula: (quantity * unitPrice) * (taxRate/100)
    /// </summary>
    public static decimal CalculateTaxAmount(decimal quantity, decimal unitPrice, decimal taxRatePercent)
    {
        var taxDecimal = GetDecimalFraction(taxRatePercent);
        return quantity * unitPrice * taxDecimal;
    }
}
