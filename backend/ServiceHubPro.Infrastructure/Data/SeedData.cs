using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceHubPro.Domain.Entities;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Entities;

namespace ServiceHubPro.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        // Check if data already exists
        if (await context.Tenants.AnyAsync())
        {
            return; // Data already seeded
        }

        // Seed Roles
        await SeedRolesAsync(roleManager);

        // Seed Tenant
        var demoTenant = await SeedTenantAsync(context);

        // Seed Users
        await SeedUsersAsync(userManager, demoTenant);

        // Seed Location
        var location = await SeedLocationAsync(context, demoTenant);

        // Seed Customers
        await SeedCustomersAsync(context, demoTenant);

        // Seed Services
        await SeedServicesAsync(context, demoTenant);

        // Seed Products
        await SeedProductsAsync(context, demoTenant, location);

        // Seed Appointments
        await SeedAppointmentsAsync(context, demoTenant, location);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = new[] { "SuperAdmin", "BusinessOwner", "Manager", "Staff", "Accountant", "Customer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }

    private static async Task<Tenant> SeedTenantAsync(ApplicationDbContext context)
    {
        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Name == "Demo Auto Care");
        if (tenant == null)
        {
            tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Demo Auto Care",
                BusinessName = "Demo Auto Care LLC",
                Email = "owner@demo.com",
                Phone = "+1-555-0100",
                Address = "123 Main Street",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                Country = "USA",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
        }
        return tenant;
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, Tenant tenant)
    {
        // Owner user
        var ownerEmail = "owner@demo.com";
        var owner = await userManager.FindByEmailAsync(ownerEmail);
        if (owner == null)
        {
            owner = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = ownerEmail,
                Email = ownerEmail,
                FirstName = "John",
                LastName = "Owner",
                TenantId = tenant.Id,
                IsActive = true,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(owner, "Password123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(owner, "BusinessOwner");
            }
        }

        // Manager user
        var managerEmail = "manager@demo.com";
        var manager = await userManager.FindByEmailAsync(managerEmail);
        if (manager == null)
        {
            manager = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = managerEmail,
                Email = managerEmail,
                FirstName = "Jane",
                LastName = "Manager",
                TenantId = tenant.Id,
                IsActive = true,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(manager, "Password123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(manager, "Manager");
            }
        }
    }

    private static async Task<Location> SeedLocationAsync(ApplicationDbContext context, Tenant tenant)
    {
        var location = await context.Locations.FirstOrDefaultAsync(l => l.TenantId == tenant.Id);
        if (location == null)
        {
            location = new Location
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = "Main Location",
                Address = "123 Main Street",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                Phone = "+1-555-0100",
                Email = "main@demo.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Locations.Add(location);
            await context.SaveChangesAsync();
        }
        return location;
    }

    private static async Task SeedCustomersAsync(ApplicationDbContext context, Tenant tenant)
    {
        if (await context.Customers.AnyAsync(c => c.TenantId == tenant.Id))
            return;

        var customers = new[]
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice.johnson@example.com",
                Phone = "+1-555-1001",
                Address = "456 Oak Avenue",
                City = "Springfield",
                State = "IL",
                ZipCode = "62702",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                FirstName = "Bob",
                LastName = "Smith",
                Email = "bob.smith@example.com",
                Phone = "+1-555-1002",
                Address = "789 Pine Road",
                City = "Springfield",
                State = "IL",
                ZipCode = "62703",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            }
        };

        context.Customers.AddRange(customers);
    }

    private static async Task SeedServicesAsync(ApplicationDbContext context, Tenant tenant)
    {
        if (await context.Services.AnyAsync(s => s.TenantId == tenant.Id))
            return;

        var services = new[]
        {
            new Service
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = "Oil Change",
                Description = "Standard oil change service",
                DurationMinutes = 30,
                Price = 49.99m,
                TaxRate = 8m, // Store as percent (8 for 8%)
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = "Tire Rotation",
                Description = "Rotate all four tires",
                DurationMinutes = 45,
                Price = 29.99m,
                TaxRate = 8m, // Store as percent (8 for 8%)
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = "Brake Inspection",
                Description = "Complete brake system inspection",
                DurationMinutes = 60,
                Price = 79.99m,
                TaxRate = 8m, // Store as percent (8 for 8%)
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Services.AddRange(services);
    }

    private static async Task SeedProductsAsync(ApplicationDbContext context, Tenant tenant, Location location)
    {
        if (await context.Products.AnyAsync(p => p.TenantId == tenant.Id))
            return;

        var products = new[]
        {
            new Product
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                LocationId = location.Id,
                Name = "Engine Oil 5W-30",
                Description = "Synthetic engine oil",
                SKU = "OIL-5W30-001",
                UnitPrice = 24.99m,
                StockQuantity = 50,
                LowStockThreshold = 10,
                Unit = "Quart",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                LocationId = location.Id,
                Name = "Oil Filter",
                Description = "Standard oil filter",
                SKU = "FILTER-OIL-001",
                UnitPrice = 8.99m,
                StockQuantity = 30,
                LowStockThreshold = 5,
                Unit = "Each",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Products.AddRange(products);
    }

    private static async Task SeedAppointmentsAsync(ApplicationDbContext context, Tenant tenant, Location location)
    {
        if (await context.Appointments.AnyAsync(a => a.TenantId == tenant.Id))
            return;

        var customers = await context.Customers.Where(c => c.TenantId == tenant.Id).ToListAsync();
        var services = await context.Services.Where(s => s.TenantId == tenant.Id).ToListAsync();
        var users = await context.Users.Where(u => u.TenantId == tenant.Id).ToListAsync();

        if (!customers.Any() || !services.Any() || !users.Any())
            return;

        var appointments = new[]
        {
            new Appointment
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                LocationId = location.Id,
                CustomerId = customers[0].Id,
                ServiceId = services[0].Id,
                StaffId = users[0].Id,
                ScheduledStart = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
                ScheduledEnd = DateTime.UtcNow.AddDays(1).Date.AddHours(10).AddMinutes(30),
                Status = "Scheduled",
                Notes = "Customer requested morning appointment",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        context.Appointments.AddRange(appointments);
    }
}
