# OrderService Microservice

A production-ready .NET microservice for managing orders with full CI/CD pipeline and Kubernetes deployment.

## 🚀 Quick Start

### Local Development

```bash
# Using Docker Compose
docker-compose up -d

# Test the API
curl http://localhost:5000/health
```

### Running Tests

```bash
cd OrderService.Tests
dotnet test
```

## 📋 Features

- **RESTful API** - Complete CRUD operations for orders
- **JWT Authentication** - Secure API endpoints
- **PostgreSQL Database** - With Entity Framework Core
- **Health Checks** - Liveness and readiness probes
- **OpenTelemetry** - Distributed tracing and metrics
- **Comprehensive Tests** - Unit and integration tests
- **Docker Support** - Multi-stage Dockerfile
- **CI/CD Pipeline** - GitHub Actions with automated deployment
- **Helm Charts** - Kubernetes deployment ready

## 🏗️ Architecture

```
OrderMicroService/
├── OrderService/          # Main API service
│   ├── Controllers/       # API endpoints
│   ├── Service/          # Business logic
│   ├── Model/            # Domain models
│   ├── Middleware/       # Custom middleware
│   └── Migrations/       # EF Core migrations
├── OrderService.Tests/   # Test project
└── OrderUI/              # Blazor frontend (optional)
```

## 🔧 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | - |
| `JwtSettings__SecretKey` | JWT secret key | - |
| `JwtSettings__Issuer` | JWT issuer | `OrderService` |
| `JwtSettings__Audience` | JWT audience | `OrderServiceClient` |
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | `Development` |

### Database Setup

```bash
# Run migrations
cd OrderService
dotnet ef database update

# Or use docker-compose (includes PostgreSQL)
docker-compose up -d
```

## 🐳 Docker

### Build Image

```bash
cd OrderService
docker build -t orderservice:latest .
```

### Run Container

```bash
docker run -d \
  -p 5000:5000 \
  -e ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=orderdb;Username=orderuser;Password=orderpass" \
  -e JwtSettings__SecretKey="your-secret-key" \
  orderservice:latest
```

## ☸️ Kubernetes Deployment

### Using Helm (Recommended)

```bash
cd ../../helm/microservices

# Deploy all microservices including OrderService
helm upgrade --install microservices . \
  --namespace microservices \
  --create-namespace \
  --values values.yaml
```

See [HELM-DEPLOYMENT-GUIDE.md](HELM-DEPLOYMENT-GUIDE.md) for detailed deployment instructions.

### Manual Kubernetes Deployment

```bash
# Apply Kubernetes manifests
kubectl apply -f k8s/

# Check deployment
kubectl get pods -l app=order-service
```

## 🔄 CI/CD Pipeline

The project includes a complete GitHub Actions pipeline:

### Pipeline Steps

1. ✅ **Build & Test** - Compiles code and runs all tests
2. 🔍 **Security Scan** - Checks for vulnerable dependencies
3. 🐳 **Docker Build** - Creates and pushes images to Docker Hub
4. 🚀 **Deploy to Staging** - Automatic deployment on `develop` branch
5. 🚀 **Deploy to Production** - Automatic deployment on `main` branch (with approval)

### Trigger Deployment

```bash
# Deploy to staging
git checkout develop
git push origin develop

# Deploy to production
git checkout main
git merge develop
git push origin main
```

See [CI-CD-DOCUMENTATION.md](CI-CD-DOCUMENTATION.md) for complete pipeline documentation.

## 📚 Documentation

- **[CI/CD Documentation](CI-CD-DOCUMENTATION.md)** - Complete CI/CD pipeline setup
- **[Helm Deployment Guide](HELM-DEPLOYMENT-GUIDE.md)** - Kubernetes deployment with Helm
- **[GHCR Publishing Guide](GHCR-PUBLISH-GUIDE.md)** - Manual Docker image publishing
- **[JWT Authentication Guide](OrderService/JWT_AUTH_GUIDE.md)** - API authentication
- **[Observability Setup](OBSERVABILITY-SETUP.md)** - Monitoring and tracing

## 🧪 Testing

### Run All Tests

```bash
cd OrderService.Tests
dotnet test
```

### Run with Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Test Specific Controller

```bash
dotnet test --filter FullyQualifiedName~ControllerTests
```

### API Testing

Use the included test scripts:

```bash
# PowerShell
.\test-api.ps1

# Bash
./curl-commands.sh
```

Or use the HTTP file with REST Client extension:

```
OrderService/OrderService.http
```

## 🔐 Security

- JWT-based authentication
- Role-based authorization
- Input validation
- SQL injection protection (EF Core)
- Security headers middleware
- Dependency vulnerability scanning in CI/CD

## 📊 Observability

### Health Checks

```bash
# Liveness probe
curl http://localhost:5000/health

# Readiness probe
curl http://localhost:5000/health/ready
```

### Metrics & Tracing

The service exports OpenTelemetry metrics and traces. Configure collectors:

```yaml
# appsettings.json
"OpenTelemetry": {
  "ServiceName": "OrderService",
  "OtlpEndpoint": "http://otel-collector:4317"
}
```

See [OBSERVABILITY-SETUP.md](OBSERVABILITY-SETUP.md) for SigNoz/Grafana integration.

## 🔧 Development

### Prerequisites

- .NET 8.0 SDK
- PostgreSQL 15+
- Docker & Docker Compose (optional)
- Visual Studio 2022 or VS Code

### Local Setup

```bash
# Clone the repository
git clone <repo-url>
cd Order/OrderMicroService

# Restore dependencies
cd OrderService
dotnet restore

# Update database
dotnet ef database update

# Run the service
dotnet run
```

### Adding New Features

1. Create feature branch: `git checkout -b feature/new-feature`
2. Implement changes in `OrderService/`
3. Add tests in `OrderService.Tests/`
4. Run tests: `dotnet test`
5. Commit and push
6. Create Pull Request to `develop`

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## 📦 Production Deployment Checklist

- [ ] Update `appsettings.Production.json` with production values
- [ ] Set strong JWT secret key
- [ ] Configure production database connection
- [ ] Enable HTTPS
- [ ] Set appropriate resource limits in Helm values
- [ ] Configure monitoring and alerting
- [ ] Set up database backups
- [ ] Review security settings
- [ ] Configure GitHub Environments with approvals
- [ ] Test rollback procedures

## 🔗 Related Services

- **[Auth Service](../../auth-service/)** - Authentication and user management
- **[Product Service](../../product-service/)** - Product catalog
- **[Helm Charts](../../helm/microservices/)** - Kubernetes deployment configurations

## 📝 License

[Your License Here]

## 👥 Team

[Your Team Information]

---

**Quick Links:**
- [API Documentation](OrderService/OrderService.http)
- [Helm Chart](../../helm/microservices/templates/services/order-csharp-service.yaml)
- [CI/CD Pipeline](.github/workflows/ci-cd.yml)
