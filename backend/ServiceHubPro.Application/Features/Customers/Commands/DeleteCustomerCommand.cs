using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Customers.Commands;

public class DeleteCustomerCommand : IRequest<ApiResponse<object>>
{
    public Guid Id { get; set; }
}

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, ApiResponse<object>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public DeleteCustomerCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<object>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<object>.ErrorResponse("Tenant context not found");
        }

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == tenantId.Value && !c.IsDeleted, cancellationToken);

        if (customer == null)
        {
            return ApiResponse<object>.ErrorResponse("Customer not found");
        }

        customer.IsDeleted = true;
        customer.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.SuccessResponse(null, "Customer deleted successfully");
    }
}
