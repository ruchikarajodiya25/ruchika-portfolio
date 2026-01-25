using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Products.Queries;

public class GetProductsQuery : IRequest<ApiResponse<PagedResult<ProductDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public Guid? LocationId { get; set; }
    public bool? LowStock { get; set; }
}

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ApiResponse<PagedResult<ProductDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public GetProductsQueryHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<PagedResult<ProductDto>>.ErrorResponse("Tenant context not found");
        }

        var query = _context.Products
            .Where(p => p.TenantId == tenantId.Value && !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                (p.SKU != null && p.SKU.ToLower().Contains(searchTerm)));
        }

        if (request.LocationId.HasValue)
        {
            query = query.Where(p => p.LocationId == request.LocationId.Value);
        }

        if (request.LowStock == true)
        {
            query = query.Where(p => p.StockQuantity <= p.LowStockThreshold);
        }

        query = query.OrderBy(p => p.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                SKU = p.SKU,
                Category = p.Category,
                UnitPrice = p.UnitPrice,
                CostPrice = p.CostPrice,
                StockQuantity = p.StockQuantity,
                LowStockThreshold = p.LowStockThreshold,
                Unit = p.Unit,
                IsActive = p.IsActive,
                LocationId = p.LocationId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ProductDto>
        {
            Items = products,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return ApiResponse<PagedResult<ProductDto>>.SuccessResponse(result);
    }
}
