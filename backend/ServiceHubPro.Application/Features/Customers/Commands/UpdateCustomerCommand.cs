using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Customers.Commands;

public class UpdateCustomerCommand : IRequest<ApiResponse<CustomerDto>>
{
    public Guid Id { get; set; }
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

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, ApiResponse<CustomerDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public UpdateCustomerCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<CustomerDto>.ErrorResponse("Tenant context not found");
        }

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == tenantId.Value && !c.IsDeleted, cancellationToken);

        if (customer == null)
        {
            return ApiResponse<CustomerDto>.ErrorResponse("Customer not found");
        }

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.Mobile = request.Mobile;
        customer.Address = request.Address;
        customer.City = request.City;
        customer.State = request.State;
        customer.ZipCode = request.ZipCode;
        customer.Country = request.Country;
        customer.DateOfBirth = request.DateOfBirth;
        customer.Notes = request.Notes;
        customer.Tags = request.Tags;
        customer.UpdatedAt = DateTime.UtcNow;

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
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };

        return ApiResponse<CustomerDto>.SuccessResponse(dto, "Customer updated successfully");
    }
}
