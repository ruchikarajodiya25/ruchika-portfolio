namespace ServiceHubPro.Application.DTOs;

public class WorkOrderItemDto
{
    public Guid Id { get; set; }
    public Guid WorkOrderId { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public Guid? ServiceId { get; set; }
    public Guid? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TotalAmount { get; set; }
}
