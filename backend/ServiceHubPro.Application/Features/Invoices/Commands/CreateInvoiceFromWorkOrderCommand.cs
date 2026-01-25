using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Domain.Entities;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Invoices.Commands;

public class CreateInvoiceFromWorkOrderCommand : IRequest<ApiResponse<InvoiceDto>>
{
    public Guid WorkOrderId { get; set; }
}

public class CreateInvoiceFromWorkOrderCommandHandler : IRequestHandler<CreateInvoiceFromWorkOrderCommand, ApiResponse<InvoiceDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public CreateInvoiceFromWorkOrderCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<InvoiceDto>> Handle(CreateInvoiceFromWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<InvoiceDto>.ErrorResponse("Tenant context not found");
        }

        var workOrder = await _context.WorkOrders
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId && w.TenantId == tenantId.Value && !w.IsDeleted, cancellationToken);

        if (workOrder == null)
        {
            return ApiResponse<InvoiceDto>.ErrorResponse("Work order not found");
        }

        if (workOrder.Status != "Completed")
        {
            return ApiResponse<InvoiceDto>.ErrorResponse("Work order must be completed before creating an invoice");
        }

        if (workOrder.InvoiceId.HasValue)
        {
            return ApiResponse<InvoiceDto>.ErrorResponse("Invoice already exists for this work order");
        }

        // Generate invoice number
        var lastInvoice = await _context.Invoices
            .Where(i => i.TenantId == tenantId.Value)
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{((lastInvoice?.CreatedAt.Date == DateTime.UtcNow.Date ? 
            await _context.Invoices.CountAsync(i => i.TenantId == tenantId.Value && 
                i.CreatedAt.Date == DateTime.UtcNow.Date, cancellationToken) : 0) + 1):D4}";

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            InvoiceNumber = invoiceNumber,
            CustomerId = workOrder.CustomerId,
            WorkOrderId = workOrder.Id,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending",
            SubTotal = workOrder.TotalAmount,
            TaxAmount = 0,
            DiscountAmount = 0,
            TotalAmount = workOrder.TotalAmount,
            CreatedAt = DateTime.UtcNow
        };

        // Create invoice items from work order items if they exist
        var workOrderItems = await _context.WorkOrderItems
            .Where(wi => wi.WorkOrderId == workOrder.Id && wi.TenantId == tenantId.Value)
            .ToListAsync(cancellationToken);

        foreach (var item in workOrderItems)
        {
            var invoiceItem = new InvoiceItem
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId.Value,
                InvoiceId = invoice.Id,
                ItemType = item.ItemType,
                ServiceId = item.ServiceId,
                ProductId = item.ProductId,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                TotalAmount = item.TotalAmount,
                CreatedAt = DateTime.UtcNow
            };
            invoice.Items.Add(invoiceItem);
        }

        // Calculate totals
        invoice.SubTotal = invoice.Items.Sum(i => i.TotalAmount);
        invoice.TaxAmount = invoice.Items.Sum(i => i.TotalAmount * i.TaxRate);
        invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount - invoice.DiscountAmount;

        _context.Invoices.Add(invoice);
        workOrder.InvoiceId = invoice.Id;
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerId = invoice.CustomerId,
            WorkOrderId = invoice.WorkOrderId,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status,
            SubTotal = invoice.SubTotal,
            TaxAmount = invoice.TaxAmount,
            DiscountAmount = invoice.DiscountAmount,
            TotalAmount = invoice.TotalAmount,
            CreatedAt = invoice.CreatedAt
        };

        return ApiResponse<InvoiceDto>.SuccessResponse(dto, "Invoice created successfully");
    }
}
