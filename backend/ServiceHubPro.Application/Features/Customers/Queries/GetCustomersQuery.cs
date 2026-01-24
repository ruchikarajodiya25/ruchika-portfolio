using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Customers.Queries;

public class GetCustomersQuery : IRequest<ApiResponse<PagedResult<CustomerDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; } = "LastName";
    public bool SortDescending { get; set; } = false;
}

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, ApiResponse<PagedResult<CustomerDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public GetCustomersQueryHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<PagedResult<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<PagedResult<CustomerDto>>.ErrorResponse("Tenant context not found");
        }

        var query = _context.Customers
            .Where(c => c.TenantId == tenantId.Value && !c.IsDeleted);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(searchTerm) ||
                c.LastName.ToLower().Contains(searchTerm) ||
                (c.Email != null && c.Email.ToLower().Contains(searchTerm)) ||
                (c.Phone != null && c.Phone.Contains(searchTerm)));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "firstname" => request.SortDescending
                ? query.OrderByDescending(c => c.FirstName)
                : query.OrderBy(c => c.FirstName),
            "email" => request.SortDescending
                ? query.OrderByDescending(c => c.Email)
                : query.OrderBy(c => c.Email),
            "createdat" => request.SortDescending
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            _ => request.SortDescending
                ? query.OrderByDescending(c => c.LastName)
                : query.OrderBy(c => c.LastName)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var customers = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                Mobile = c.Mobile,
                Address = c.Address,
                City = c.City,
                State = c.State,
                ZipCode = c.ZipCode,
                Country = c.Country,
                DateOfBirth = c.DateOfBirth,
                Notes = c.Notes,
                Tags = c.Tags,
                TotalSpent = c.TotalSpent,
                TotalVisits = c.TotalVisits,
                LastVisitAt = c.LastVisitAt,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<CustomerDto>
        {
            Items = customers,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return ApiResponse<PagedResult<CustomerDto>>.SuccessResponse(result);
    }
}
