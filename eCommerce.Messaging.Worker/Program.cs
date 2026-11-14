using eCommerce.Application;
using eCommerce.Infrastructure;
using eCommerce.Infrastructure.Messaging.AzureServiceBus;
using eCommerce.Infrastructure.Messaging;
using eCommerce.Messaging.Worker.Consumers;
using eCommerce.Messaging.Worker.HostedServices;
using eCommerce.Messaging.Worker;
using Serilog;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilog((services, configuration) => configuration
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(MessagingTelemetry.ActivitySourceName)
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddMeter(MessagingTelemetry.Meter.Name)
        .AddOtlpExporter());
builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DatabaseConnection") ?? string.Empty,
    builder.Configuration);
builder.Services.AddTransient<IOrderCreatedAction, EmailNotificationAction>();
builder.Services.AddTransient<IOrderCreatedAction, InventoryManagementAction>();
builder.Services.AddTransient<IOrderCreatedAction, OrderAuditAction>();
builder.Services.AddHostedService<OrderEventProcessor>();

var host = builder.Build();
host.Run();
