namespace ServiceHubPro.Infrastructure.Services;

public interface ITenantContextService
{
    Guid? GetCurrentTenantId();
    void SetCurrentTenantId(Guid tenantId);
}
