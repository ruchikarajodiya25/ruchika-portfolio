namespace ServiceHubPro.Application.DTOs;

public class CreateWorkOrderItemDto
{
    public string ItemType { get; set; } = string.Empty; // "Service", "Product", "Labor"
    public Guid? ServiceId { get; set; }
    public Guid? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; } // Tax rate as percent (0-100), e.g., 8 for 8%
}
