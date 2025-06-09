using BuildingBlocks.Messaging.MassTransit;
using Discount.Grpc;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
var programAssembly = typeof(Program).Assembly;
var connectionString = builder.Configuration.GetConnectionString("Database");
var redis = builder.Configuration.GetConnectionString("Redis");

// Application Services
builder.Services.AddCarter();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(programAssembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// Data Services
builder.Services.AddMarten(options =>
{
    options.Connection(connectionString!);
    options.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redis;
});

// GRPC Services
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
    return handler;
});

// Async Communication Services
builder.Services.AddMessageBroker(builder.Configuration);

// Cross-Cutting Services
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString!)
    .AddRedis(redis!);

var app = builder.Build();

app.MapCarter();
app.UseExceptionHandler(options => { });
app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
app.Run();