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

public class CreateInvoiceFromWorkOrderCommandHandler : IRequestHandler<CreateInvoiceFromWorkOrderCommand, ApiResponse<InvoiceDto>>
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

    public async Task<ApiResponse<InvoiceDto>> Handle(CreateInvoiceFromWorkOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContextService.GetCurrentTenantId();
            if (!tenantId.HasValue)
            {
                return ApiResponse<InvoiceDto>.ErrorResponse("Tenant context not found");
            }

            // Load work order with items and related entities
            var workOrder = await _context.WorkOrders
                .Include(w => w.Items)
                .Include(w => w.Invoice)
                .Include(w => w.Customer)
                .Include(w => w.Location)
                .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId && w.TenantId == tenantId.Value && !w.IsDeleted, cancellationToken);

            if (workOrder == null)
            {
                return ApiResponse<InvoiceDto>.ErrorResponse("Work order not found");
            }

            if (workOrder.Status != "Completed")
            {
                return ApiResponse<InvoiceDto>.ErrorResponse("Work order must be completed before creating an invoice");
            }

            if (workOrder.Invoice != null)
            {
                return ApiResponse<InvoiceDto>.ErrorResponse("Invoice already exists for this work order");
            }

            // Ensure work order has items (filter out deleted items)
            var activeItems = workOrder.Items?.Where(i => !i.IsDeleted).ToList() ?? new List<WorkOrderItem>();
            if (!activeItems.Any())
            {
                return ApiResponse<InvoiceDto>.ErrorResponse("Work order must have at least one item before creating an invoice");
            }

            // Validate items
            foreach (var item in activeItems)
            {
                if (item.Quantity <= 0)
                {
                    return ApiResponse<InvoiceDto>.ErrorResponse($"Item '{item.Description}' has invalid quantity: {item.Quantity}");
                }
                if (item.UnitPrice < 0)
                {
                    return ApiResponse<InvoiceDto>.ErrorResponse($"Item '{item.Description}' has invalid unit price: {item.UnitPrice}");
                }
            }

            // Generate invoice number
            var lastInvoice = await _context.Invoices
                .Where(i => i.TenantId == tenantId.Value)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{((lastInvoice?.CreatedAt.Date == DateTime.UtcNow.Date ? 
                await _context.Invoices.CountAsync(i => i.TenantId == tenantId.Value && 
                    i.CreatedAt.Date == DateTime.UtcNow.Date, cancellationToken) : 0) + 1):D4}";

            // Calculate totals from work order items using tax rate as percent
            decimal subTotal = 0;
            decimal taxAmount = 0;
            
            foreach (var item in activeItems)
            {
                var itemSubTotal = item.Quantity * item.UnitPrice;
                // TaxRate is stored as percent (0-100), convert to decimal for calculation
                var taxDecimal = TaxRateHelper.GetDecimalFraction(item.TaxRate);
                var itemTax = itemSubTotal * taxDecimal;
                subTotal += itemSubTotal;
                taxAmount += itemTax;
            }

            var totalAmount = subTotal + taxAmount;

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId.Value,
                InvoiceNumber = invoiceNumber,
                CustomerId = workOrder.CustomerId,
                LocationId = workOrder.LocationId,
                WorkOrderId = workOrder.Id,
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                Status = "Draft",
                SubTotal = subTotal,
                TaxAmount = taxAmount,
                DiscountAmount = 0,
                TotalAmount = totalAmount,
                PaidAmount = 0,
                CreatedAt = DateTime.UtcNow
            };

            // Create invoice items from work order items (only active items)
            foreach (var item in activeItems)
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
                    TaxRate = item.TaxRate, // Store as percent (0-100)
                    TotalAmount = TaxRateHelper.CalculateItemTotal(item.Quantity, item.UnitPrice, item.TaxRate),
                    CreatedAt = DateTime.UtcNow
                };
                invoice.Items.Add(invoiceItem);
            }

            // Recalculate totals from invoice items to ensure accuracy
            invoice.SubTotal = invoice.Items.Sum(i => i.Quantity * i.UnitPrice);
            invoice.TaxAmount = invoice.Items.Sum(i => TaxRateHelper.CalculateTaxAmount(i.Quantity, i.UnitPrice, i.TaxRate));
            invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount - invoice.DiscountAmount;

            _context.Invoices.Add(invoice);
            
            await _context.SaveChangesAsync(cancellationToken);

            // Load invoice with items for DTO mapping
            var savedInvoice = await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id, cancellationToken);

            if (savedInvoice == null)
            {
                return ApiResponse<InvoiceDto>.ErrorResponse("Failed to retrieve created invoice");
            }

            var dto = new InvoiceDto
            {
                Id = savedInvoice.Id,
                InvoiceNumber = savedInvoice.InvoiceNumber,
                CustomerId = savedInvoice.CustomerId,
                CustomerName = savedInvoice.Customer != null ? $"{savedInvoice.Customer.FirstName} {savedInvoice.Customer.LastName}" : string.Empty,
                WorkOrderId = savedInvoice.WorkOrderId,
                InvoiceDate = savedInvoice.InvoiceDate,
                DueDate = savedInvoice.DueDate,
                Status = savedInvoice.Status,
                SubTotal = savedInvoice.SubTotal,
                TaxAmount = savedInvoice.TaxAmount,
                DiscountAmount = savedInvoice.DiscountAmount,
                TotalAmount = savedInvoice.TotalAmount,
                PaidAmount = savedInvoice.PaidAmount,
                Items = savedInvoice.Items.Select(i => new InvoiceItemDto
                {
                    Id = i.Id,
                    ItemType = i.ItemType,
                    ServiceId = i.ServiceId,
                    ProductId = i.ProductId,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TaxRate = i.TaxRate,
                    TotalAmount = i.TotalAmount
                }).ToList(),
                CreatedAt = savedInvoice.CreatedAt
            };

            return ApiResponse<InvoiceDto>.SuccessResponse(dto, "Invoice created successfully");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating invoice from work order {WorkOrderId}", request.WorkOrderId);
            var innerException = ex.InnerException?.Message ?? ex.Message;
            return ApiResponse<InvoiceDto>.ErrorResponse($"Failed to create invoice: {innerException}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice from work order {WorkOrderId}", request.WorkOrderId);
            return ApiResponse<InvoiceDto>.ErrorResponse($"An error occurred while creating the invoice: {ex.Message}");
        }
    }
}
