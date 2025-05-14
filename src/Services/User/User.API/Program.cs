using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PaymentProcessor.BuildingBlocks.EventBus.Extensions;
using PaymentProcessor.BuildingBlocks.EventBus.Kafka;
using PaymentProcessor.BuildingBlocks.Infrastructure.Extensions;
using PaymentProcessor.Services.User.API.EventHandlers;
using PaymentProcessor.Services.User.API.Events;
using PaymentProcessor.Services.User.Infrastructure.Data;
using PaymentProcessor.Services.User.Infrastructure.Extensions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User API", Version = "v1" });
    
    // Add JWT authentication to Swagger
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

// Add infrastructure services (including database)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? "default-secret-key-for-development-only"))
    };
});

// Add Kafka Event Bus
var kafkaSettings = new KafkaConnectionSettings
{
    BootstrapServers = builder.Configuration.GetValue<string>("Kafka:BootstrapServers") ?? "localhost:9092",
    ClientId = "user-service",
    ConsumerGroupId = "user-service-group",
    DefaultTopic = "payment-events"
};
builder.Services.AddEventBus(kafkaSettings);

// Add event handlers
builder.Services.AddScoped<UserRegisteredEventHandler>();

// Add AWS services
builder.Services.AddAwsServices(builder.Configuration);

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

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    await DbInitializer.InitializeAsync(dbContext);

    // Subscribe to events
    var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
    eventBus.Subscribe<UserRegisteredEvent, UserRegisteredEventHandler>();
}

app.Run();