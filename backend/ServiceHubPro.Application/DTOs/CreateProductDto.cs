namespace ServiceHubPro.Application.DTOs;

public class CreateProductDto
{
    public Guid LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public string? Category { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; } = 0;
    public int LowStockThreshold { get; set; } = 10;
    public string? Unit { get; set; }
    public bool IsActive { get; set; } = true;
}
