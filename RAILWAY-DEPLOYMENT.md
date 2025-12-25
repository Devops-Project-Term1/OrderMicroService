# Railway Deployment Guide for OrderService

## Overview

Railway is a modern PaaS that makes deployment simple. This guide shows how to deploy OrderService to Railway with CI/CD.

## Setup Steps

### 1. Create Railway Account

1. Go to https://railway.app
2. Sign up with GitHub
3. Verify your account

### 2. Create Railway Project

**Option A: Using Railway Dashboard**

1. Click "New Project"
2. Select "Deploy from GitHub repo"
3. Choose your OrderService repository
4. Railway will auto-detect the Dockerfile

**Option B: Using Railway CLI**

```bash
# Install Railway CLI
npm install -g @railway/cli

# Login
railway login

# Initialize project (in OrderService directory)
cd C:/DevOps/Order/OrderMicroService/OrderService
railway init

# Link to your project
railway link
```

### 3. Add PostgreSQL Database

1. In Railway dashboard, click "+ New"
2. Select "Database" → "PostgreSQL"
3. Railway automatically creates the database
4. Connection string is available as `DATABASE_URL`

### 4. Configure Environment Variables

In Railway dashboard → Your Service → Variables:

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT

# Database (Railway auto-provides DATABASE_URL, convert it)
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}

# JWT Settings
JwtSettings__SecretKey=your-super-secret-jwt-key-change-this-in-production
JwtSettings__Issuer=OrderService
JwtSettings__Audience=OrderServiceClient
JwtSettings__ExpirationMinutes=60

# Auth Service (if needed)
AUTH_SERVICE_URL=https://your-auth-service.railway.app
```

**Important:** Railway uses `$PORT` variable. Update your OrderService to listen on this port.

### 5. Update Program.cs (if needed)

Make sure OrderService listens on Railway's PORT:

```csharp
// Program.cs
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
```

### 6. Get Railway Token for CI/CD

1. Go to Railway → Account Settings → Tokens
2. Click "Create New Token"
3. Copy the token (starts with `railway_`)

### 7. Add GitHub Secrets

In your GitHub repository → Settings → Secrets:

| Secret | Value |
|--------|-------|
| `RAILWAY_TOKEN` | Token from step 6 |
| `RAILWAY_PRODUCTION_URL` | Your Railway service URL (e.g., `https://orderservice.railway.app`) |

### 8. Create Railway Services

You need two services for staging and production:

**Using Railway CLI:**

```bash
# Create staging service
railway service create orderservice-staging

# Create production service  
railway service create orderservice-production
```

**Or in Dashboard:**
1. Create two separate projects: "orderservice-staging" and "orderservice-production"
2. Each gets its own PostgreSQL database

## Railway Configuration File

Create `railway.json` in OrderService root:

```json
{
  "build": {
    "builder": "DOCKERFILE",
    "dockerfilePath": "Dockerfile"
  },
  "deploy": {
    "startCommand": "dotnet OrderService.dll",
    "healthcheckPath": "/health",
    "healthcheckTimeout": 100,
    "restartPolicyType": "ON_FAILURE",
    "restartPolicyMaxRetries": 10
  }
}
```

Or `railway.toml`:

```toml
[build]
builder = "DOCKERFILE"
dockerfilePath = "Dockerfile"

[deploy]
startCommand = "dotnet OrderService.dll"
healthcheckPath = "/health"
healthcheckTimeout = 100
restartPolicyType = "ON_FAILURE"
restartPolicyMaxRetries = 10
```

## CI/CD Workflow

The updated `.github/workflows/ci-cd.yml` now deploys to Railway:

- Push to `develop` → Deploy to Railway staging
- Push to `main` → Deploy to Railway production

## Manual Deployment

### Using Railway CLI

```bash
# Deploy to staging
railway link orderservice-staging
railway up

# Deploy to production
railway link orderservice-production
railway up

# Or specify service
railway up --service orderservice-production
```

### Using GitHub Integration

Railway can auto-deploy on every push:

1. Railway Dashboard → Service → Settings
2. Enable "Auto Deploy" from GitHub
3. Choose branch (main/develop)

## Environment-Specific Configurations

### Staging Environment

```bash
# In Railway dashboard for staging service
ASPNETCORE_ENVIRONMENT=Staging
JwtSettings__ExpirationMinutes=120
```

### Production Environment

```bash
# In Railway dashboard for production service
ASPNETCORE_ENVIRONMENT=Production
JwtSettings__ExpirationMinutes=60
```

## Database Migrations

Railway runs migrations automatically if you add this to your Dockerfile:

```dockerfile
# Add after COPY
RUN dotnet ef database update
```

Or run manually:

```bash
# Using Railway CLI
railway run dotnet ef database update
```

## Monitoring & Logs

### View Logs

```bash
# Using CLI
railway logs

# Or specify service
railway logs --service orderservice-production
```

### In Dashboard
- Go to your service → "Deployments" → Click deployment → View logs

## Custom Domain

1. Railway Dashboard → Service → Settings → Domains
2. Click "Generate Domain" or "Custom Domain"
3. Add your domain and configure DNS

## Pricing

Railway offers:
- **Free Tier**: $5 credit/month, great for testing
- **Developer Plan**: $20/month for production
- **Team Plan**: $20/user/month

## Advantages of Railway

✅ Zero-config deployments
✅ Automatic HTTPS
✅ Built-in PostgreSQL
✅ Simple environment variables
✅ Automatic scaling
✅ GitHub integration
✅ Great for .NET, Node.js, Python, etc.

## Migration from Kubernetes to Railway

If you want to move from Kubernetes:

1. Deploy to Railway (automated via CI/CD)
2. Update DNS to point to Railway
3. No need for Helm charts, Kubernetes configs
4. Railway handles infrastructure

## Troubleshooting

### Port Issues

Make sure your app listens on `$PORT`:

```csharp
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
```

### Database Connection

Railway provides `DATABASE_URL`. Convert to .NET format:

```bash
# Railway format:
postgresql://user:pass@host:5432/dbname

# .NET format:
Host=host;Port=5432;Database=dbname;Username=user;Password=pass
```

### Build Failures

Check Dockerfile is in correct location:
```bash
railway logs --deployment
```

## Complete Example

1. **Create Railway projects** (staging & production)
2. **Add PostgreSQL** to each
3. **Set environment variables**
4. **Add RAILWAY_TOKEN** to GitHub secrets
5. **Push to develop** → Auto-deploy to staging
6. **Merge to main** → Auto-deploy to production

## Next Steps

1. Set up custom domain
2. Configure monitoring with Railway metrics
3. Add database backups
4. Set up alerts for failures
5. Configure auto-scaling

## Comparison: Railway vs Kubernetes

| Feature | Railway | Kubernetes |
|---------|---------|------------|
| Setup | 5 minutes | Hours/Days |
| Complexity | Low | High |
| Cost | $20-50/month | Variable (can be higher) |
| Scaling | Automatic | Manual config |
| Database | Built-in | Deploy yourself |
| Best For | Small/Medium apps | Large enterprise |

Railway is perfect for OrderService deployment - simple, fast, and production-ready!
