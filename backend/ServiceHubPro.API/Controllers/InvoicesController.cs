using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHubPro.Application.Common.Models;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Application.Features.Invoices.Commands;
using ServiceHubPro.Application.Features.Invoices.Queries;
using System.Text;

namespace ServiceHubPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InvoiceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<InvoiceDto>>>> GetInvoices(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? customerId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetInvoicesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            CustomerId = customerId,
            Status = status,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetInvoice(Guid id)
    {
        var query = new GetInvoiceByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoicePdf(Guid id)
    {
        var query = new GetInvoiceByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.Success || result.Data == null)
        {
            return NotFound();
        }

        var invoice = result.Data;
        var html = GenerateInvoiceHtml(invoice);
        var pdfBytes = GeneratePdfFromHtml(html);

        return File(pdfBytes, "application/pdf", $"Invoice-{invoice.InvoiceNumber}.pdf");
    }

    [HttpPost("from-workorder/{workOrderId}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> CreateInvoiceFromWorkOrder(Guid workOrderId)
    {
        var command = new CreateInvoiceFromWorkOrderCommand { WorkOrderId = workOrderId };
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetInvoices), new { id = result.Data!.Id }, result);
    }

    private string GenerateInvoiceHtml(InvoiceDto invoice)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset='utf-8' />");
        html.AppendLine("<style>");
        html.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; }");
        html.AppendLine(".header { text-align: center; margin-bottom: 30px; }");
        html.AppendLine(".invoice-info { display: flex; justify-content: space-between; margin-bottom: 30px; }");
        html.AppendLine(".info-section { flex: 1; }");
        html.AppendLine("table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }");
        html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        html.AppendLine("th { background-color: #f2f2f2; }");
        html.AppendLine(".total-section { text-align: right; margin-top: 20px; }");
        html.AppendLine(".total-row { margin: 5px 0; }");
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<div class='header'>");
        html.AppendLine("<h1>INVOICE</h1>");
        html.AppendLine($"<p>Invoice #: {invoice.InvoiceNumber}</p>");
        html.AppendLine("</div>");
        html.AppendLine("<div class='invoice-info'>");
        html.AppendLine("<div class='info-section'>");
        html.AppendLine("<h3>Bill To:</h3>");
        html.AppendLine($"<p>{invoice.CustomerName}</p>");
        html.AppendLine("</div>");
        html.AppendLine("<div class='info-section'>");
        html.AppendLine("<h3>Invoice Details:</h3>");
        html.AppendLine($"<p>Date: {invoice.InvoiceDate:MMM dd, yyyy}</p>");
        if (invoice.DueDate.HasValue)
        {
            html.AppendLine($"<p>Due Date: {invoice.DueDate.Value:MMM dd, yyyy}</p>");
        }
        html.AppendLine($"<p>Status: {invoice.Status}</p>");
        html.AppendLine("</div>");
        html.AppendLine("</div>");
        html.AppendLine("<table>");
        html.AppendLine("<thead>");
        html.AppendLine("<tr>");
        html.AppendLine("<th>Description</th>");
        html.AppendLine("<th>Quantity</th>");
        html.AppendLine("<th>Unit Price</th>");
        html.AppendLine("<th>Total</th>");
        html.AppendLine("</tr>");
        html.AppendLine("</thead>");
        html.AppendLine("<tbody>");
        foreach (var item in invoice.Items)
        {
            html.AppendLine("<tr>");
            html.AppendLine($"<td>{item.Description}</td>");
            html.AppendLine($"<td>{item.Quantity}</td>");
            html.AppendLine($"<td>${item.UnitPrice:F2}</td>");
            html.AppendLine($"<td>${item.TotalAmount:F2}</td>");
            html.AppendLine("</tr>");
        }
        html.AppendLine("</tbody>");
        html.AppendLine("</table>");
        html.AppendLine("<div class='total-section'>");
        html.AppendLine($"<div class='total-row'><strong>Subtotal: ${invoice.SubTotal:F2}</strong></div>");
        if (invoice.DiscountAmount > 0)
        {
            html.AppendLine($"<div class='total-row'>Discount: ${invoice.DiscountAmount:F2}</div>");
        }
        html.AppendLine($"<div class='total-row'>Tax: ${invoice.TaxAmount:F2}</div>");
        html.AppendLine($"<div class='total-row'><strong>Total: ${invoice.TotalAmount:F2}</strong></div>");
        html.AppendLine($"<div class='total-row'>Paid: ${invoice.PaidAmount:F2}</div>");
        html.AppendLine($"<div class='total-row'><strong>Balance: ${(invoice.TotalAmount - invoice.PaidAmount):F2}</strong></div>");
        html.AppendLine("</div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return html.ToString();
    }

    private byte[] GeneratePdfFromHtml(string html)
    {
        // For now, return HTML as plain text
        // In production, use a library like QuestPDF, iTextSharp, or DinkToPdf
        // This is a placeholder that returns HTML content
        // TODO: Integrate a proper PDF library
        return Encoding.UTF8.GetBytes(html);
    }
}
