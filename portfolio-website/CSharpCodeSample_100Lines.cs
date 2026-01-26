// C# Code Sample - ServiceHub Pro Project
// Demonstrates: Clean Architecture, CQRS Pattern, Entity Framework Core, Multi-Tenancy
// Author: Ruchika Rajodiya

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceHubPro.Application.Common.Helpers;
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

public class CreateInvoiceFromWorkOrderCommandHandler 
    : IRequestHandler<CreateInvoiceFromWorkOrderCommand, ApiResponse<InvoiceDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;
    private readonly ILogger<CreateInvoiceFromWorkOrderCommandHandler> _logger;

    public CreateInvoiceFromWorkOrderCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService,
        ILogger<CreateInvoiceFromWorkOrderCommandHandler> logger)
    {
        _context = context;
        _tenantContextService = tenantContextService;
        _logger = logger;
    }

    public async Task<ApiResponse<InvoiceDto>> Handle(
        CreateInvoiceFromWorkOrderCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContextService.GetCurrentTenantId();
            if (!tenantId.HasValue)
                return ApiResponse<InvoiceDto>.ErrorResponse("Tenant context not found");

            var workOrder = await _context.WorkOrders
                .Include(w => w.Items).Include(w => w.Invoice).Include(w => w.Customer)
                .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId && w.TenantId == tenantId.Value && !w.IsDeleted, cancellationToken);

            if (workOrder == null)
                return ApiResponse<InvoiceDto>.ErrorResponse("Work order not found");
            if (workOrder.Status != "Completed")
                return ApiResponse<InvoiceDto>.ErrorResponse("Work order must be completed");
            if (workOrder.Invoice != null)
                return ApiResponse<InvoiceDto>.ErrorResponse("Invoice already exists");

            var activeItems = workOrder.Items?.Where(i => !i.IsDeleted).ToList() ?? new List<WorkOrderItem>();
            if (!activeItems.Any())
                return ApiResponse<InvoiceDto>.ErrorResponse("Work order must have at least one item");

            foreach (var item in activeItems)
                if (item.Quantity <= 0 || item.UnitPrice < 0)
                    return ApiResponse<InvoiceDto>.ErrorResponse($"Item '{item.Description}' has invalid data");

            var lastInvoice = await _context.Invoices.Where(i => i.TenantId == tenantId.Value)
                .OrderByDescending(i => i.CreatedAt).FirstOrDefaultAsync(cancellationToken);

            var count = lastInvoice?.CreatedAt.Date == DateTime.UtcNow.Date 
                ? await _context.Invoices.CountAsync(i => i.TenantId == tenantId.Value && i.CreatedAt.Date == DateTime.UtcNow.Date, cancellationToken) 
                : 0;
            var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D4}";

            decimal subTotal = 0, taxAmount = 0;
            foreach (var item in activeItems)
            {
                var itemSubTotal = item.Quantity * item.UnitPrice;
                subTotal += itemSubTotal;
                taxAmount += itemSubTotal * TaxRateHelper.GetDecimalFraction(item.TaxRate);
            }

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(), TenantId = tenantId.Value, InvoiceNumber = invoiceNumber,
                CustomerId = workOrder.CustomerId, LocationId = workOrder.LocationId, WorkOrderId = workOrder.Id,
                InvoiceDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30), Status = "Draft",
                SubTotal = subTotal, TaxAmount = taxAmount, TotalAmount = subTotal + taxAmount, CreatedAt = DateTime.UtcNow
            };

            foreach (var item in activeItems)
                invoice.Items.Add(new InvoiceItem
                {
                    Id = Guid.NewGuid(), TenantId = tenantId.Value, InvoiceId = invoice.Id,
                    ItemType = item.ItemType, Description = item.Description, Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice, TaxRate = item.TaxRate,
                    TotalAmount = TaxRateHelper.CalculateItemTotal(item.Quantity, item.UnitPrice, item.TaxRate),
                    CreatedAt = DateTime.UtcNow
                });

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync(cancellationToken);

            var savedInvoice = await _context.Invoices.Include(i => i.Items).Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id, cancellationToken);

            var dto = new InvoiceDto
            {
                Id = savedInvoice!.Id, InvoiceNumber = savedInvoice.InvoiceNumber,
                CustomerId = savedInvoice.CustomerId, WorkOrderId = savedInvoice.WorkOrderId,
                InvoiceDate = savedInvoice.InvoiceDate, DueDate = savedInvoice.DueDate, Status = savedInvoice.Status,
                SubTotal = savedInvoice.SubTotal, TaxAmount = savedInvoice.TaxAmount, TotalAmount = savedInvoice.TotalAmount,
                Items = savedInvoice.Items.Select(i => new InvoiceItemDto
                {
                    Id = i.Id, ItemType = i.ItemType, Description = i.Description,
                    Quantity = i.Quantity, UnitPrice = i.UnitPrice, TaxRate = i.TaxRate, TotalAmount = i.TotalAmount
                }).ToList()
            };

            return ApiResponse<InvoiceDto>.SuccessResponse(dto, "Invoice created successfully");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating invoice from work order {WorkOrderId}", request.WorkOrderId);
            return ApiResponse<InvoiceDto>.ErrorResponse($"Failed to create invoice: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice from work order {WorkOrderId}", request.WorkOrderId);
            return ApiResponse<InvoiceDto>.ErrorResponse($"An error occurred: {ex.Message}");
        }
    }
}
