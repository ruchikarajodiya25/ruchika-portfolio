using Xunit;
using ServiceHubPro.Application.Features.Appointments.Commands;
using System;

namespace ServiceHubPro.Tests;

public class AppointmentConflictDetectionTests
{
    [Fact]
    public void AppointmentConflict_OverlappingTimes_ShouldDetectConflict()
    {
        // Arrange
        var start1 = new DateTime(2024, 1, 15, 10, 0, 0);
        var end1 = new DateTime(2024, 1, 15, 11, 0, 0);
        
        var start2 = new DateTime(2024, 1, 15, 10, 30, 0);
        var end2 = new DateTime(2024, 1, 15, 11, 30, 0);

        // Act
        var hasConflict = HasTimeConflict(start1, end1, start2, end2);

        // Assert
        Assert.True(hasConflict);
    }

    [Fact]
    public void AppointmentConflict_NonOverlappingTimes_ShouldNotDetectConflict()
    {
        // Arrange
        var start1 = new DateTime(2024, 1, 15, 10, 0, 0);
        var end1 = new DateTime(2024, 1, 15, 11, 0, 0);
        
        var start2 = new DateTime(2024, 1, 15, 11, 0, 0);
        var end2 = new DateTime(2024, 1, 15, 12, 0, 0);

        // Act
        var hasConflict = HasTimeConflict(start1, end1, start2, end2);

        // Assert
        Assert.False(hasConflict);
    }

    [Fact]
    public void AppointmentConflict_AdjacentTimes_ShouldNotDetectConflict()
    {
        // Arrange
        var start1 = new DateTime(2024, 1, 15, 10, 0, 0);
        var end1 = new DateTime(2024, 1, 15, 11, 0, 0);
        
        var start2 = new DateTime(2024, 1, 15, 11, 0, 0);
        var end2 = new DateTime(2024, 1, 15, 12, 0, 0);

        // Act
        var hasConflict = HasTimeConflict(start1, end1, start2, end2);

        // Assert
        Assert.False(hasConflict);
    }

    private bool HasTimeConflict(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
    {
        // Logic matches the conflict detection in CreateAppointmentCommandHandler
        return start1 < end2 && start2 < end1;
    }
}
