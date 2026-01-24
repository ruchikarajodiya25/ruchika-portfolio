using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Locations.Queries;

public class GetLocationsQuery : IRequest<ApiResponse<PagedResult<LocationDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool? IsActive { get; set; }
}

public class GetLocationsQueryHandler : IRequestHandler<GetLocationsQuery, ApiResponse<PagedResult<LocationDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public GetLocationsQueryHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<PagedResult<LocationDto>>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<PagedResult<LocationDto>>.ErrorResponse("Tenant context not found");
        }

        var query = _context.Locations
            .Where(l => l.TenantId == tenantId.Value && !l.IsDeleted);

        if (request.IsActive.HasValue)
        {
            query = query.Where(l => l.IsActive == request.IsActive.Value);
        }

        query = query.OrderBy(l => l.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var locations = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new LocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Address = l.Address,
                City = l.City,
                State = l.State,
                ZipCode = l.ZipCode,
                Phone = l.Phone,
                Email = l.Email,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<LocationDto>
        {
            Items = locations,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return ApiResponse<PagedResult<LocationDto>>.SuccessResponse(result);
    }
}
