using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Domain.Entities;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Services;

namespace ServiceHubPro.Application.Features.Payments.Commands;

public class CreatePaymentCommand : IRequest<ApiResponse<PaymentDto>>
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Card, Online, Check, Other
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
}

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, ApiResponse<PaymentDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextService _tenantContextService;

    public CreatePaymentCommandHandler(
        ApplicationDbContext context,
        ITenantContextService tenantContextService)
    {
        _context = context;
        _tenantContextService = tenantContextService;
    }

    public async Task<ApiResponse<PaymentDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return ApiResponse<PaymentDto>.ErrorResponse("Tenant context not found");
        }

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId && i.TenantId == tenantId.Value && !i.IsDeleted, cancellationToken);

        if (invoice == null)
        {
            return ApiResponse<PaymentDto>.ErrorResponse("Invoice not found");
        }

        // Check if payment exceeds invoice balance
        var totalPaid = await _context.Payments
            .Where(p => p.InvoiceId == request.InvoiceId && !p.IsDeleted)
            .SumAsync(p => p.Amount, cancellationToken);

        if (totalPaid + request.Amount > invoice.TotalAmount)
        {
            return ApiResponse<PaymentDto>.ErrorResponse($"Payment amount exceeds invoice balance. Remaining balance: ${(invoice.TotalAmount - totalPaid):F2}");
        }

        // Generate payment number
        var lastPayment = await _context.Payments
            .Where(p => p.TenantId == tenantId.Value)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var paymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{((lastPayment?.CreatedAt.Date == DateTime.UtcNow.Date ? 
            await _context.Payments.CountAsync(p => p.TenantId == tenantId.Value && 
                p.CreatedAt.Date == DateTime.UtcNow.Date, cancellationToken) : 0) + 1):D4}";

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            InvoiceId = request.InvoiceId,
            PaymentNumber = paymentNumber,
            PaymentDate = request.PaymentDate,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);

        // Update invoice paid amount
        invoice.PaidAmount = totalPaid + request.Amount;
        if (invoice.PaidAmount >= invoice.TotalAmount)
        {
            invoice.Status = "Paid";
        }
        else if (invoice.PaidAmount > 0)
        {
            invoice.Status = "PartiallyPaid";
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new PaymentDto
        {
            Id = payment.Id,
            PaymentNumber = payment.PaymentNumber,
            InvoiceId = payment.InvoiceId,
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes,
            CreatedAt = payment.CreatedAt
        };

        return ApiResponse<PaymentDto>.SuccessResponse(dto, "Payment recorded successfully");
    }
}
