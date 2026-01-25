using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceHubPro.Application.Common.Helpers;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Domain.Entities;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.WorkOrders.Commands;

public class RemoveWorkOrderItemCommand : IRequest<ApiResponse<bool>>
{
    public Guid WorkOrderId { get; set; }
    public Guid ItemId { get; set; }
}

public class RemoveWorkOrderItemCommandHandler : IRequestHandler<RemoveWorkOrderItemCommand, ApiResponse<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;
    private readonly ILogger<RemoveWorkOrderItemCommandHandler> _logger;

    public RemoveWorkOrderItemCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService,
        ILogger<RemoveWorkOrderItemCommandHandler> logger)
    {
        _context = context;
        _tenantContextService = tenantContextService;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> Handle(RemoveWorkOrderItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContextService.GetCurrentTenantId();
            if (!tenantId.HasValue)
            {
                return ApiResponse<bool>.ErrorResponse("Tenant context not found");
            }

            // Load work order with items
            var workOrder = await _context.WorkOrders
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId && w.TenantId == tenantId.Value && !w.IsDeleted, cancellationToken);

            if (workOrder == null)
            {
                return ApiResponse<bool>.ErrorResponse("Work order not found");
            }

            // Find and soft delete the item
            var item = workOrder.Items.FirstOrDefault(i => i.Id == request.ItemId && !i.IsDeleted);
            if (item == null)
            {
                return ApiResponse<bool>.ErrorResponse("Work order item not found");
            }

            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;

            // Recalculate work order total
            var newTotal = workOrder.Items
                .Where(i => !i.IsDeleted)
                .Sum(item => TaxRateHelper.CalculateItemTotal(item.Quantity, item.UnitPrice, item.TaxRate));
            workOrder.TotalAmount = newTotal;

            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.SuccessResponse(true, "Work order item removed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing work order item {ItemId} from work order {WorkOrderId}", request.ItemId, request.WorkOrderId);
            return ApiResponse<bool>.ErrorResponse($"Failed to remove item: {ex.Message}");
        }
    }
}
