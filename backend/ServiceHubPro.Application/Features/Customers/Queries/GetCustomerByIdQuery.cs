using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Customers.Queries;

public class GetCustomerByIdQuery : IRequest<ApiResponse<CustomerDto>>
{
    public Guid Id { get; set; }
}

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, ApiResponse<CustomerDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public GetCustomerByIdQueryHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<CustomerDto>.ErrorResponse("Tenant context not found");
        }

        var customer = await _context.Customers
            .Where(c => c.Id == request.Id && c.TenantId == tenantId.Value && !c.IsDeleted)
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
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (customer == null)
        {
            return ApiResponse<CustomerDto>.ErrorResponse("Customer not found");
        }

        return ApiResponse<CustomerDto>.SuccessResponse(customer);
    }
}
