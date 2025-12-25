# OpenTelemetry & Observability Setup

## 🎯 Overview
This OrderService is now fully instrumented with **OpenTelemetry** for comprehensive observability, including:
- **Traces**: Distributed tracing of all HTTP requests and database queries
- **Metrics**: Runtime metrics, HTTP metrics, and process metrics
- **Logs**: Structured logging with OpenTelemetry integration

## 📦 Components

### OpenTelemetry Packages
The following packages have been added to `OrderService.csproj`:
```xml
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.11.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.11.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.11.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.11.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.11.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Process" Version="0.5.0-beta.7" />
<PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.0.0-beta.12" />
```

### Instrumentation Enabled
1. **ASP.NET Core Instrumentation**
   - Automatic tracing of HTTP requests
   - Exception recording
   - Filters out health check and Swagger endpoints

2. **HTTP Client Instrumentation**
   - Traces outbound HTTP calls
   - Records exceptions

3. **Entity Framework Core Instrumentation**
   - Traces database queries
   - Includes SQL statements in traces
   - Enriches with query details

4. **Runtime Instrumentation**
   - GC metrics
   - Thread pool metrics
   - Memory usage

5. **Process Instrumentation**
   - CPU usage
   - Memory usage
   - Process metrics

## 🎨 Visualization: Jaeger

Jaeger is configured as the telemetry backend for visualizing traces, metrics, and logs.

### Access Jaeger UI
- **URL**: http://localhost:16686
- **Service Name**: OrderService

### Features
- View distributed traces
- Analyze request latency
- Identify performance bottlenecks
- Search traces by operation, tags, duration

## 🚀 Running the Stack

### Start All Services
```powershell
docker-compose up -d
```

This will start:
1. **PostgreSQL**: Database (port 5432)
2. **Jaeger**: Observability platform (UI on port 16686)
3. **OrderService**: Your API (port 5000)

### Check Status
```powershell
docker-compose ps
```

### View Logs
```powershell
# OrderService logs
docker-compose logs orderservice

# Jaeger logs
docker-compose logs jaeger

# Follow all logs
docker-compose logs -f
```

## 📊 Generating Telemetry Data

### Make API Requests
```powershell
# This will generate traces (returns 401 due to auth)
curl http://localhost:5000/api/orders

# With JWT token (once you have one)
curl http://localhost:5000/api/orders -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### View in Jaeger
1. Open http://localhost:16686
2. Select "OrderService" from the Service dropdown
3. Click "Find Traces"
4. Click on any trace to see detailed span information

## 🔧 Configuration

### Environment Variables (docker-compose.yml)
```yaml
OpenTelemetry__OtlpEndpoint: "http://jaeger:4317"
```

### appsettings.json
```json
{
  "OpenTelemetry": {
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

## 📈 What Gets Traced

### HTTP Requests
- Request method, path, headers
- Response status code
- Request duration
- User claims (if authenticated)

### Database Queries
- SQL statements
- Query parameters
- Execution time
- Connection information

### Custom Spans
You can add custom spans in your code:
```csharp
using System.Diagnostics;

var activity = Activity.Current;
activity?.SetTag("custom.tag", "value");
activity?.AddEvent(new ActivityEvent("Custom Event"));
```

## 🎯 Metrics Available

1. **HTTP Metrics**
   - Request count
   - Request duration
   - Active requests

2. **Runtime Metrics**
   - GC collections
   - Heap size
   - Thread count

3. **Process Metrics**
   - CPU usage
   - Memory usage
   - Thread pool usage

## 🔍 Sample Trace Structure

```
HTTP GET /api/orders
├── ASP.NET Core: Authorization
├── Entity Framework Core: Database Query
│   ├── SQL: SELECT * FROM Orders
│   └── Duration: 45ms
└── Response: 200 OK (or 401 Unauthorized)
Total Duration: 123ms
```

## 📝 Next Steps

### Upgrade to SigNoz (Optional)
For a more feature-rich observability platform with better metrics and log management:

1. Replace Jaeger with SigNoz in docker-compose.yml
2. Update OTLP endpoint
3. Access SigNoz UI at http://localhost:3301

### Add Custom Instrumentation
```csharp
// In your service methods
using var activity = ActivitySource.StartActivity("CustomOperation");
activity?.SetTag("order.id", orderId);
activity?.SetTag("user.id", userId);
// Your code here
```

### Add Alerts
Configure alerts in Jaeger/SigNoz for:
- High error rates
- Slow queries
- High latency endpoints

## 🛠️ Troubleshooting

### No Traces Appearing
1. Check Jaeger is running: `docker-compose ps`
2. Verify OTLP endpoint: `echo $env:OpenTelemetry__OtlpEndpoint`
3. Check OrderService logs for errors

### High Memory Usage
OpenTelemetry uses batching. Adjust in Program.cs:
```csharp
.AddOtlpExporter(options =>
{
    options.Endpoint = new Uri(otlpEndpoint);
    options.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
    {
        MaxQueueSize = 2048,
        ScheduledDelayMilliseconds = 5000,
        ExporterTimeoutMilliseconds = 30000,
        MaxExportBatchSize = 512
    };
})
```

## 📚 Resources
- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/languages/net/)
- [Jaeger Documentation](https://www.jaegertracing.io/docs/)
- [OTLP Specification](https://opentelemetry.io/docs/specs/otlp/)
