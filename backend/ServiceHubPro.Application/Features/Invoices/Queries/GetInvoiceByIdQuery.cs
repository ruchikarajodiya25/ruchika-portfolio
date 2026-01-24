using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Invoices.Queries;

public class GetInvoiceByIdQuery : IRequest<ApiResponse<InvoiceDto>>
{
    public Guid Id { get; set; }
}

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, ApiResponse<InvoiceDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public GetInvoiceByIdQueryHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<InvoiceDto>.ErrorResponse("Tenant context not found");
        }

        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.Id && i.TenantId == tenantId.Value && !i.IsDeleted, cancellationToken);

        if (invoice == null)
        {
            return ApiResponse<InvoiceDto>.ErrorResponse("Invoice not found");
        }

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
            PaidAmount = invoice.PaidAmount,
            Items = invoice.Items.Select(item => new InvoiceItemDto
            {
                Id = item.Id,
                ItemType = item.ItemType,
                ServiceId = item.ServiceId,
                ProductId = item.ProductId,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                TotalAmount = item.TotalAmount
            }).ToList(),
            CreatedAt = invoice.CreatedAt
        };

        return ApiResponse<InvoiceDto>.SuccessResponse(dto);
    }
}
