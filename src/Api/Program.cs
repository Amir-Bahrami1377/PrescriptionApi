using System.Reflection;
using System.Text;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Prescription.Api.Auth;
using Prescription.Api.Extensions;
using Prescription.Api.Middleware;
using Prescription.Api.OpenApi;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Catalog;
using Prescription.Modules.Catalog.Infrastructure.Persistence;
using Prescription.Modules.Consultation;
using Prescription.Modules.Consultation.Infrastructure.Persistence;
using Prescription.Modules.FileStorage;
using Prescription.Modules.Identity;
using Prescription.Modules.Identity.Infrastructure.Persistence;
using Prescription.Modules.Notifications;
using Prescription.Modules.Orders;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.Modules.Payments;
using Prescription.Modules.Ticketing;
using Prescription.Modules.Ticketing.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Behaviors;
using Prescription.SharedKernel.Security;
using Scalar.AspNetCore;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console());

// Every Feature slice across every module contributes its own MediatR handler/validator/endpoint;
// this is the single list of module assemblies the composition root scans.
var moduleAssemblies = new[]
{
    typeof(Prescription.Modules.Identity.IdentityModule).Assembly,
    typeof(Prescription.Modules.Catalog.CatalogModule).Assembly,
    typeof(Prescription.Modules.Orders.OrdersModule).Assembly,
    typeof(Prescription.Modules.Consultation.ConsultationModule).Assembly,
    typeof(Prescription.Modules.Ticketing.TicketingModule).Assembly,
};

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddSingleton<IColumnEncryptor>(_ => new AesColumnEncryptor(new ColumnEncryptionOptions
{
    Key = builder.Configuration["ColumnEncryption:Key"]
        ?? throw new InvalidOperationException("ColumnEncryption:Key is not configured."),
}));

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("ConnectionStrings:Redis is not configured.");
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(moduleAssemblies);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

foreach (var assembly in moduleAssemblies)
{
    builder.Services.AddValidatorsFromAssembly(assembly);
}

builder.Services.AddEndpointsFromAssemblies(moduleAssemblies);

var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres is not configured.");

builder.Services.AddIdentityModule(builder.Configuration, postgresConnectionString);
builder.Services.AddCatalogModule(postgresConnectionString);
builder.Services.AddOrdersModule(postgresConnectionString);
builder.Services.AddPaymentsModule(builder.Configuration);
builder.Services.AddFileStorageModule(builder.Configuration);
builder.Services.AddConsultationModule(postgresConnectionString);
builder.Services.AddTicketingModule(postgresConnectionString);
builder.Services.AddNotificationsModule(builder.Configuration);

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHangfire(config => config
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(postgresConnectionString)));
builder.Services.AddHangfireServer();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddNpgSql(postgresConnectionString)
    .AddRedis(redisConnectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var migrationScope = app.Services.CreateScope();
    await migrationScope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.MigrateAsync();
    await migrationScope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
    await migrationScope.ServiceProvider.GetRequiredService<OrdersDbContext>().Database.MigrateAsync();
    await migrationScope.ServiceProvider.GetRequiredService<ConsultationDbContext>().Database.MigrateAsync();
    await migrationScope.ServiceProvider.GetRequiredService<TicketingDbContext>().Database.MigrateAsync();
}

app.UseExceptionHandler(_ => { });

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapDiscoveredEndpoints();
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard();
}

app.Run();

public partial class Program;
