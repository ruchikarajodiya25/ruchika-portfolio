using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Domain.Entities;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Customers.Commands;

public class CreateCustomerCommand : IRequest<ApiResponse<CustomerDto>>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Notes { get; set; }
    public string? Tags { get; set; }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, ApiResponse<CustomerDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public CreateCustomerCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<CustomerDto>.ErrorResponse("Tenant context not found");
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Mobile = request.Mobile,
            Address = request.Address,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            Country = request.Country,
            DateOfBirth = request.DateOfBirth,
            Notes = request.Notes,
            Tags = request.Tags,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            Mobile = customer.Mobile,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            ZipCode = customer.ZipCode,
            Country = customer.Country,
            DateOfBirth = customer.DateOfBirth,
            Notes = customer.Notes,
            Tags = customer.Tags,
            TotalSpent = customer.TotalSpent,
            TotalVisits = customer.TotalVisits,
            LastVisitAt = customer.LastVisitAt,
            CreatedAt = customer.CreatedAt
        };

        return ApiResponse<CustomerDto>.SuccessResponse(dto, "Customer created successfully");
    }
}
