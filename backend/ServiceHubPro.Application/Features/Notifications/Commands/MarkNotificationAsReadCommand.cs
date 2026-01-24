using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Notifications.Commands;

public class MarkNotificationAsReadCommand : IRequest<ApiResponse<object>>
{
    public Guid Id { get; set; }
}

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, ApiResponse<object>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public MarkNotificationAsReadCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<object>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<object>.ErrorResponse("Tenant context not found");
        }

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.TenantId == tenantId.Value && !n.IsDeleted, cancellationToken);

        if (notification == null)
        {
            return ApiResponse<object>.ErrorResponse("Notification not found");
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.SuccessResponse(null, "Notification marked as read");
    }
}
