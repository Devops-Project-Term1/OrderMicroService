using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Services;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;

var builder = WebApplication.CreateBuilder(args);

// Configure port for Railway deployment
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 1. Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Register Services
builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();

// HTTP Clients for inter-service communication
var productServiceUrl = builder.Configuration["ServiceUrls:ProductService"] ?? "http://localhost:3000";
var stockServiceUrl = builder.Configuration["ServiceUrls:StockService"] ?? "http://localhost:5000";

builder.Services.AddHttpClient<IProductService, ProductService>(client =>
{
    client.BaseAddress = new Uri(productServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IStockService, StockService>(client =>
{
    client.BaseAddress = new Uri(stockServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// 3. OpenTelemetry Configuration
var serviceName = "OrderService";
var serviceVersion = "1.0.0";
var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = httpContext => 
            {
                // Don't trace health check or swagger endpoints
                var path = httpContext.Request.Path.Value;
                return !(path?.Contains("/swagger") == true || path?.Contains("/health") == true);
            };
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
        })
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.EnrichWithIDbCommand = (activity, command) =>
            {
                activity.SetTag("db.query", command.CommandText);
            };
        })
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
        }));

// 4. Logging with OpenTelemetry
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri(otlpEndpoint);
    });
});

// 5. CORS Configuration - Allow all origins (no authentication required)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying migrations: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only use HTTPS redirection in production if not behind a reverse proxy (Railway handles HTTPS)
// app.UseHttpsRedirection();

// Middleware Pipeline
app.UseCors();

// Health check endpoint for Railway
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Controllers/Routes
app.MapControllers();

app.Run();