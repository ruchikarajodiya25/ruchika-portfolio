using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Services.Commands;

public class DeleteServiceCommand : IRequest<ApiResponse<object>>
{
    public Guid Id { get; set; }
}

public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, ApiResponse<object>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public DeleteServiceCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<object>> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<object>.ErrorResponse("Tenant context not found");
        }

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.TenantId == tenantId.Value && !s.IsDeleted, cancellationToken);

        if (service == null)
        {
            return ApiResponse<object>.ErrorResponse("Service not found");
        }

        service.IsDeleted = true;
        service.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.SuccessResponse(null, "Service deleted successfully");
    }
}
