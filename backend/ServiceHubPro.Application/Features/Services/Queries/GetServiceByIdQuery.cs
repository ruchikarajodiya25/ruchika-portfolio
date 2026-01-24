using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Services.Queries;

public class GetServiceByIdQuery : IRequest<ApiResponse<ServiceDto>>
{
    public Guid Id { get; set; }
}

public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ApiResponse<ServiceDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public GetServiceByIdQueryHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<ServiceDto>> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<ServiceDto>.ErrorResponse("Tenant context not found");
        }

        var service = await _context.Services
            .Where(s => s.Id == request.Id && s.TenantId == tenantId.Value && !s.IsDeleted)
            .Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes,
                Category = s.Category,
                TaxCategory = s.TaxCategory,
                TaxRate = s.TaxRate,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (service == null)
        {
            return ApiResponse<ServiceDto>.ErrorResponse("Service not found");
        }

        return ApiResponse<ServiceDto>.SuccessResponse(service);
    }
}
