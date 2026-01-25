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

public class AddWorkOrderItemCommand : IRequest<ApiResponse<WorkOrderItemDto>>
{
    public Guid WorkOrderId { get; set; }
    public CreateWorkOrderItemDto Item { get; set; } = null!;
}

public class AddWorkOrderItemCommandHandler : IRequestHandler<AddWorkOrderItemCommand, ApiResponse<WorkOrderItemDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;
    private readonly ILogger<AddWorkOrderItemCommandHandler> _logger;

    public AddWorkOrderItemCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService,
        ILogger<AddWorkOrderItemCommandHandler> logger)
    {
        _context = context;
        _tenantContextService = tenantContextService;
        _logger = logger;
    }

    public async Task<ApiResponse<WorkOrderItemDto>> Handle(AddWorkOrderItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContextService.GetCurrentTenantId();
            if (!tenantId.HasValue)
            {
                return ApiResponse<WorkOrderItemDto>.ErrorResponse("Tenant context not found");
            }

            // Load work order
            var workOrder = await _context.WorkOrders
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId && w.TenantId == tenantId.Value && !w.IsDeleted, cancellationToken);

            if (workOrder == null)
            {
                return ApiResponse<WorkOrderItemDto>.ErrorResponse("Work order not found");
            }

            // Validate item data
            if (request.Item.Quantity <= 0)
            {
                return ApiResponse<WorkOrderItemDto>.ErrorResponse("Quantity must be greater than 0");
            }

            if (request.Item.UnitPrice < 0)
            {
                return ApiResponse<WorkOrderItemDto>.ErrorResponse("Unit price cannot be negative");
            }

            // Validate tax rate is between 0 and 100 (percent)
            if (request.Item.TaxRate < 0 || request.Item.TaxRate > 100)
            {
                return ApiResponse<WorkOrderItemDto>.ErrorResponse("Tax rate must be between 0 and 100");
            }

            // Calculate item total using tax rate as percent
            var itemTotal = TaxRateHelper.CalculateItemTotal(
                request.Item.Quantity,
                request.Item.UnitPrice,
                request.Item.TaxRate
            );

            // Create work order item - store tax rate as percent (0-100)
            var workOrderItem = new WorkOrderItem
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId.Value,
                WorkOrderId = workOrder.Id,
                ItemType = request.Item.ItemType,
                ServiceId = request.Item.ServiceId,
                ProductId = request.Item.ProductId,
                Description = request.Item.Description,
                Quantity = request.Item.Quantity,
                UnitPrice = request.Item.UnitPrice,
                TaxRate = request.Item.TaxRate, // Store as percent (0-100)
                TotalAmount = itemTotal,
                CreatedAt = DateTime.UtcNow
            };

            _context.WorkOrderItems.Add(workOrderItem);

            // Recalculate work order total
            workOrder.Items.Add(workOrderItem);
            var newTotal = workOrder.Items
                .Where(i => !i.IsDeleted)
                .Sum(item => TaxRateHelper.CalculateItemTotal(item.Quantity, item.UnitPrice, item.TaxRate));
            workOrder.TotalAmount = newTotal;

            await _context.SaveChangesAsync(cancellationToken);

            var dto = new WorkOrderItemDto
            {
                Id = workOrderItem.Id,
                WorkOrderId = workOrderItem.WorkOrderId,
                ItemType = workOrderItem.ItemType,
                ServiceId = workOrderItem.ServiceId,
                ProductId = workOrderItem.ProductId,
                Description = workOrderItem.Description,
                Quantity = workOrderItem.Quantity,
                UnitPrice = workOrderItem.UnitPrice,
                TaxRate = workOrderItem.TaxRate,
                TotalAmount = workOrderItem.TotalAmount
            };

            return ApiResponse<WorkOrderItemDto>.SuccessResponse(dto, "Work order item added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding work order item to work order {WorkOrderId}", request.WorkOrderId);
            return ApiResponse<WorkOrderItemDto>.ErrorResponse($"Failed to add item: {ex.Message}");
        }
    }
}
