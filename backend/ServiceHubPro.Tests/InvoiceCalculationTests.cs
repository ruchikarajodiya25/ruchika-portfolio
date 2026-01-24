using Xunit;

namespace ServiceHubPro.Tests;

public class InvoiceCalculationTests
{
    [Fact]
    public void InvoiceCalculation_WithTax_ShouldCalculateCorrectly()
    {
        // Arrange
        decimal subTotal = 100.00m;
        decimal taxRate = 0.10m; // 10%
        decimal discountAmount = 10.00m;

        // Act
        decimal taxAmount = subTotal * taxRate;
        decimal totalAmount = subTotal + taxAmount - discountAmount;

        // Assert
        Assert.Equal(10.00m, taxAmount);
        Assert.Equal(100.00m, totalAmount);
    }

    [Fact]
    public void InvoiceCalculation_WithoutDiscount_ShouldCalculateCorrectly()
    {
        // Arrange
        decimal subTotal = 100.00m;
        decimal taxRate = 0.08m; // 8%

        // Act
        decimal taxAmount = subTotal * taxRate;
        decimal totalAmount = subTotal + taxAmount;

        // Assert
        Assert.Equal(8.00m, taxAmount);
        Assert.Equal(108.00m, totalAmount);
    }

    [Fact]
    public void InvoiceCalculation_WithMultipleItems_ShouldSumCorrectly()
    {
        // Arrange
        var items = new[]
        {
            new { Quantity = 2, UnitPrice = 25.00m },
            new { Quantity = 3, UnitPrice = 10.00m },
            new { Quantity = 1, UnitPrice = 20.00m }
        };

        // Act
        decimal subTotal = items.Sum(item => item.Quantity * item.UnitPrice);

        // Assert
        Assert.Equal(100.00m, subTotal);
    }
}
