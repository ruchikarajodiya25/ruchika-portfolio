using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Helpers;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Domain.Entities;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.WorkOrders.Queries;

public class GetWorkOrdersQuery : IRequest<ApiResponse<PagedResult<WorkOrderDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? AssignedToUserId { get; set; }
}

public class GetWorkOrdersQueryHandler : IRequestHandler<GetWorkOrdersQuery, ApiResponse<PagedResult<WorkOrderDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public GetWorkOrdersQueryHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<PagedResult<WorkOrderDto>>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<PagedResult<WorkOrderDto>>.ErrorResponse("Tenant context not found");
        }

        var query = _context.WorkOrders
            .Include(w => w.Items.Where(i => !i.IsDeleted))
            .Include(w => w.Customer)
            .Include(w => w.Location)
            .Where(w => w.TenantId == tenantId.Value && !w.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(w => w.Status == request.Status);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(w => w.CustomerId == request.CustomerId.Value);
        }

        if (request.AssignedToUserId.HasValue)
        {
            query = query.Where(w => w.AssignedToUserId == request.AssignedToUserId.Value);
        }

        query = query.OrderByDescending(w => w.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var workOrders = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs and calculate totals from items
        var workOrderDtos = workOrders.Select(w => 
        {
            // Filter out deleted items
            var activeItems = w.Items?.Where(i => !i.IsDeleted).ToList() ?? new List<WorkOrderItem>();
            
            // Calculate totals from items using the formula:
            // subtotal = sum(qty * unitPrice)
            // taxTotal = sum((qty * unitPrice) * (taxRatePercent/100))
            // total = subtotal + taxTotal
            decimal subtotal = 0;
            decimal taxTotal = 0;
            decimal calculatedTotal = 0;
            
            if (activeItems.Any())
            {
                foreach (var item in activeItems)
                {
                    var itemSubtotal = item.Quantity * item.UnitPrice;
                    // TaxRate is stored as percent (0-100), convert to decimal for calculation
                    var taxDecimal = TaxRateHelper.GetDecimalFraction(item.TaxRate);
                    var itemTax = itemSubtotal * taxDecimal;
                    
                    subtotal += itemSubtotal;
                    taxTotal += itemTax;
                }
                calculatedTotal = subtotal + taxTotal;
            }
            else if (w.TotalAmount > 0)
            {
                // Fallback to stored TotalAmount if no items exist
                calculatedTotal = w.TotalAmount;
            }

            return new WorkOrderDto
            {
                Id = w.Id,
                WorkOrderNumber = w.WorkOrderNumber,
                CustomerId = w.CustomerId,
                CustomerName = w.Customer != null ? $"{w.Customer.FirstName} {w.Customer.LastName}" : string.Empty,
                AppointmentId = w.AppointmentId,
                AssignedToUserId = w.AssignedToUserId,
                LocationId = w.LocationId,
                LocationName = w.Location?.Name,
                Status = w.Status,
                Description = w.Description,
                InternalNotes = w.InternalNotes,
                TotalAmount = calculatedTotal, // Always use calculated total from items
                StartedAt = w.StartedAt,
                CompletedAt = w.CompletedAt,
                CreatedAt = w.CreatedAt,
                Items = activeItems.Select(item => new WorkOrderItemDto
                {
                    Id = item.Id,
                    WorkOrderId = item.WorkOrderId,
                    ItemType = item.ItemType,
                    ServiceId = item.ServiceId,
                    ProductId = item.ProductId,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TaxRate = item.TaxRate, // Return as percent (0-100)
                    TotalAmount = TaxRateHelper.CalculateItemTotal(item.Quantity, item.UnitPrice, item.TaxRate)
                }).ToList()
            };
        }).ToList();

        var result = new PagedResult<WorkOrderDto>
        {
            Items = workOrderDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return ApiResponse<PagedResult<WorkOrderDto>>.SuccessResponse(result);
    }
}
