using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.WorkOrders.Commands;

public class DeleteWorkOrderCommand : IRequest<ApiResponse<object>>
{
    public Guid Id { get; set; }
}

public class DeleteWorkOrderCommandHandler : IRequestHandler<DeleteWorkOrderCommand, ApiResponse<object>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public DeleteWorkOrderCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<object>> Handle(DeleteWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<object>.ErrorResponse("Tenant context not found");
        }

        var workOrder = await _context.WorkOrders
            .Include(w => w.Invoice)
            .FirstOrDefaultAsync(w => w.Id == request.Id && w.TenantId == tenantId.Value && !w.IsDeleted, cancellationToken);

        if (workOrder == null)
        {
            return ApiResponse<object>.ErrorResponse("Work order not found");
        }

        // Don't allow deletion if invoice exists
        if (workOrder.Invoice != null)
        {
            return ApiResponse<object>.ErrorResponse("Cannot delete work order that has an associated invoice");
        }

        workOrder.IsDeleted = true;
        workOrder.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.SuccessResponse(null, "Work order deleted successfully");
    }
}
