using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json.Serialization;
using ServiceHubPro.API.Middleware;
using ServiceHubPro.Application;
using ServiceHubPro.Domain.Entities;
using ServiceHubPro.Infrastructure;
using ServiceHubPro.Infrastructure.BackgroundJobs;
using ServiceHubPro.Infrastructure.Data;
using ServiceHubPro.Infrastructure.Entities;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ServiceHub Pro API",
        Version = "v1",
        Description = "Multi-Tenant Business Operations Platform API"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Register background services
builder.Services.AddHostedService<AppointmentReminderService>();
builder.Services.AddHostedService<LowStockAlertService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceHub Pro API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// Add exception handling middleware (should be early in pipeline)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Only redirect to HTTPS in Production (disable for local dev to allow HTTP requests)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // Check for pending migrations and apply them
        var pendingMigrations = context.Database.GetPendingMigrations().ToList();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying {Count} pending migration(s)...", pendingMigrations.Count);
            context.Database.Migrate();
            logger.LogInformation("Migrations applied successfully");
        }
        else if (!context.Database.CanConnect())
        {
            logger.LogError("Database does not exist and no migrations available. Please run migrations first.");
            throw new InvalidOperationException("Database connection failed. Run 'dotnet ef database update' to create the database.");
        }
        else
        {
            logger.LogInformation("Database is up to date");
        }

        // Seed data
        await SeedData.SeedAsync(context, userManager, roleManager);
        logger.LogInformation("Database seeding completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "CRITICAL: Failed to migrate or seed database. Application will not start.");
        throw; // Fail fast - don't start the app if DB setup fails
    }
}

app.Run();
