using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class WorkOrderStatusHistory : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid WorkOrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Guid? ChangedByUserId { get; set; }
    
    // Navigation properties
    public WorkOrder WorkOrder { get; set; } = null!;
    // Note: ApplicationUser navigation is in Infrastructure layer to maintain Clean Architecture
}
