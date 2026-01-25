using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Domain.Entities;

namespace ServiceHubPro.Infrastructure.BackgroundJobs;

public class LowStockAlertService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LowStockAlertService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

    public LowStockAlertService(
        IServiceProvider serviceProvider,
        ILogger<LowStockAlertService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckLowStockProducts(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in low stock alert service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckLowStockProducts(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Check if database is ready
        if (!await context.Database.CanConnectAsync(cancellationToken))
        {
            _logger.LogWarning("Database not available, skipping low stock check");
            return;
        }

        // Verify Products table exists
        try
        {
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                _logger.LogWarning("Database migrations pending, skipping low stock check");
                return;
            }
        }
        catch
        {
            _logger.LogWarning("Could not check migrations, skipping low stock check");
            return;
        }

        // Get products with low stock
        var lowStockProducts = await context.Products
            .Where(p => !p.IsDeleted && 
                       p.IsActive &&
                       p.StockQuantity <= p.LowStockThreshold)
            .GroupBy(p => p.TenantId)
            .ToListAsync(cancellationToken);

        foreach (var tenantGroup in lowStockProducts)
        {
            var tenantId = tenantGroup.Key;
            var products = tenantGroup.ToList();

            foreach (var product in products)
            {
                // Check if notification already exists for this product
                var existingNotification = await context.Notifications
                    .FirstOrDefaultAsync(n => 
                        n.TenantId == tenantId &&
                        n.RelatedEntityType == "Product" &&
                        n.RelatedEntityId == product.Id &&
                        n.Type == "LowStock" &&
                        !n.IsDeleted &&
                        n.CreatedAt > DateTime.UtcNow.AddHours(-24), // Only if created in last 24 hours
                        cancellationToken);

                if (existingNotification == null)
                {
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        Type = "LowStock",
                        Title = "Low Stock Alert",
                        Message = $"Product '{product.Name}' is running low. Current stock: {product.StockQuantity}",
                        LinkUrl = $"/inventory",
                        RelatedEntityType = "Product",
                        RelatedEntityId = product.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Notifications.Add(notification);
                    _logger.LogInformation("Created low stock notification for product {ProductId}", product.Id);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
