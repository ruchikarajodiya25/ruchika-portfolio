using ServiceHubPro.Domain.Common;

namespace ServiceHubPro.Domain.Entities;

public class WorkOrderAttachment : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid WorkOrderId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public Guid? UploadedByUserId { get; set; }
    
    // Navigation properties
    public WorkOrder WorkOrder { get; set; } = null!;
    // Note: ApplicationUser navigation is in Infrastructure layer to maintain Clean Architecture
}
