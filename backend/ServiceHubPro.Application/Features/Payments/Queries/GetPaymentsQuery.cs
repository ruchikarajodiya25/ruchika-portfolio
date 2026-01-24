using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Payments.Queries;

public class GetPaymentsQuery : IRequest<ApiResponse<PagedResult<PaymentDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? InvoiceId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? PaymentMethod { get; set; }
}

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, ApiResponse<PagedResult<PaymentDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public GetPaymentsQueryHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<PagedResult<PaymentDto>>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<PagedResult<PaymentDto>>.ErrorResponse("Tenant context not found");
        }

        var query = _context.Payments
            .Where(p => p.TenantId == tenantId.Value && !p.IsDeleted);

        if (request.InvoiceId.HasValue)
        {
            query = query.Where(p => p.InvoiceId == request.InvoiceId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= request.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
        {
            query = query.Where(p => p.PaymentMethod == request.PaymentMethod);
        }

        query = query.OrderByDescending(p => p.PaymentDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var payments = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                PaymentNumber = p.PaymentNumber,
                InvoiceId = p.InvoiceId,
                PaymentDate = p.PaymentDate,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                ReferenceNumber = p.ReferenceNumber,
                Notes = p.Notes,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<PaymentDto>
        {
            Items = payments,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return ApiResponse<PagedResult<PaymentDto>>.SuccessResponse(result);
    }
}
