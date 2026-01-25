using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Domain.Entities;

namespace ServiceHubPro.Infrastructure.BackgroundJobs;

public class AppointmentReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppointmentReminderService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public AppointmentReminderService(
        IServiceProvider serviceProvider,
        ILogger<AppointmentReminderService> logger)
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
                await CheckAndSendReminders(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in appointment reminder service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckAndSendReminders(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Check if database is ready
        if (!await context.Database.CanConnectAsync(cancellationToken))
        {
            _logger.LogWarning("Database not available, skipping appointment reminder check");
            return;
        }

        // Verify Appointments table exists
        try
        {
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                _logger.LogWarning("Database migrations pending, skipping appointment reminder check");
                return;
            }
        }
        catch
        {
            _logger.LogWarning("Could not check migrations, skipping appointment reminder check");
            return;
        }

        // Get appointments scheduled for tomorrow
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var appointments = await context.Appointments
            .Where(a => !a.IsDeleted && 
                       a.Status == "Scheduled" &&
                       a.ScheduledStart.Date == tomorrow)
            .ToListAsync(cancellationToken);

        foreach (var appointment in appointments)
        {
            // Create notification for appointment reminder
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                TenantId = appointment.TenantId,
                Type = "AppointmentReminder",
                Title = "Appointment Reminder",
                Message = $"You have an appointment scheduled for {appointment.ScheduledStart:MMM dd, yyyy HH:mm}",
                LinkUrl = $"/appointments/{appointment.Id}",
                RelatedEntityType = "Appointment",
                RelatedEntityId = appointment.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);
            _logger.LogInformation("Created reminder notification for appointment {AppointmentId}", appointment.Id);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
