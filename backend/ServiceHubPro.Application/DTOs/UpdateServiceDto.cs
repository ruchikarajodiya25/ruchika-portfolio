namespace ServiceHubPro.Application.DTOs;

public class UpdateServiceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public string? Category { get; set; }
    public string? TaxCategory { get; set; }
    public decimal TaxRate { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
