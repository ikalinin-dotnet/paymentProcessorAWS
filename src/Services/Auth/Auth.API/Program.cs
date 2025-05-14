// src/Services/Auth/Auth.API/Program.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using PaymentProcessor.BuildingBlocks.EventBus.Extensions;
using PaymentProcessor.BuildingBlocks.EventBus.Kafka;
using PaymentProcessor.BuildingBlocks.Infrastructure.Extensions;
using PaymentProcessor.Services.Auth.API.Events;
using PaymentProcessor.Services.Auth.Domain.Entities;
using PaymentProcessor.Services.Auth.Infrastructure.Data;
using PaymentProcessor.Services.Auth.Infrastructure.Extensions;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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

// Add infrastructure services (including identity and database)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add AWS services if needed
builder.Services.AddAwsServices(builder.Configuration);

// Add Kafka Event Bus
var kafkaSettings = new KafkaConnectionSettings
{
    BootstrapServers = builder.Configuration.GetValue<string>("Kafka:BootstrapServers") ?? "localhost:9092",
    ClientId = "auth-service",
    ConsumerGroupId = "auth-service-group",
    DefaultTopic = "payment-events"
};
builder.Services.AddEventBus(kafkaSettings);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    await dbContext.Database.EnsureCreatedAsync();
    
    // Seed roles
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    
    if (!await roleManager.RoleExistsAsync("User"))
    {
        await roleManager.CreateAsync(new IdentityRole("User"));
    }
    
    // Seed admin user
    if (await userManager.FindByEmailAsync("admin@example.com") == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

app.Run();