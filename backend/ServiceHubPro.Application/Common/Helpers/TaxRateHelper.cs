namespace ServiceHubPro.Application.Common.Helpers;

/// <summary>
/// Helper class for tax rate calculations.
/// Tax rates are stored as percent (0-100), e.g., 8 for 8%.
/// When calculating, divide by 100 to get decimal fraction.
/// </summary>
public static class TaxRateHelper
{
    /// <summary>
    /// Converts tax rate from percent to decimal fraction for calculations.
    /// Examples:
    /// - 8 => 0.08
    /// - 5 => 0.05
    /// - 0 => 0
    /// </summary>
    public static decimal GetDecimalFraction(decimal taxRatePercent)
    {
        return taxRatePercent / 100m;
    }

    /// <summary>
    /// Calculates the total amount for an item including tax.
    /// Formula: (quantity * unitPrice) * (1 + taxRatePercent/100)
    /// </summary>
    public static decimal CalculateItemTotal(decimal quantity, decimal unitPrice, decimal taxRatePercent)
    {
        var taxDecimal = GetDecimalFraction(taxRatePercent);
        return quantity * unitPrice * (1m + taxDecimal);
    }

    /// <summary>
    /// Calculates tax amount for an item.
    /// Formula: (quantity * unitPrice) * (taxRatePercent/100)
    /// </summary>
    public static decimal CalculateTaxAmount(decimal quantity, decimal unitPrice, decimal taxRatePercent)
    {
        var taxDecimal = GetDecimalFraction(taxRatePercent);
        return quantity * unitPrice * taxDecimal;
    }
}
