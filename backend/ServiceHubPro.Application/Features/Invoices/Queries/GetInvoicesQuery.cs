using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Invoices.Queries;

public class GetInvoicesQuery : IRequest<ApiResponse<PagedResult<InvoiceDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? CustomerId { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, ApiResponse<PagedResult<InvoiceDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public GetInvoicesQueryHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<PagedResult<InvoiceDto>>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<PagedResult<InvoiceDto>>.ErrorResponse("Tenant context not found");
        }

        var query = _context.Invoices
            .Where(i => i.TenantId == tenantId.Value && !i.IsDeleted);

        if (request.CustomerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == request.CustomerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(i => i.Status == request.Status);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate <= request.EndDate.Value);
        }

        query = query.OrderByDescending(i => i.InvoiceDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var invoices = await query
            .Include(i => i.Items)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new InvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                CustomerId = i.CustomerId,
                WorkOrderId = i.WorkOrderId,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                Status = i.Status,
                SubTotal = i.SubTotal,
                TaxAmount = i.TaxAmount,
                DiscountAmount = i.DiscountAmount,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                Items = i.Items.Select(item => new InvoiceItemDto
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
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<InvoiceDto>
        {
            Items = invoices,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return ApiResponse<PagedResult<InvoiceDto>>.SuccessResponse(result);
    }
}
