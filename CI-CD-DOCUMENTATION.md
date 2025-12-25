# CI/CD Pipeline Documentation

## Overview

This project uses GitHub Actions for continuous integration and continuous deployment (CI/CD). The pipeline includes:

- **Build & Test**: Builds the .NET application and runs unit tests
- **Code Coverage**: Collects and uploads code coverage metrics
- **Security Scan**: Checks for vulnerable dependencies
- **Docker Build**: Creates and pushes Docker images
- **Deployment**: Deploys to your infrastructure

## Workflow Details

### Trigger Events

The pipeline automatically runs on:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches

### Jobs

#### 1. Build and Test
- Restores NuGet dependencies
- Builds the project in Release configuration
- Runs unit tests with xUnit
- Generates test reports
- Uploads coverage metrics to Codecov

#### 2. Security Scan
- Checks for vulnerable NuGet packages
- Runs independently of build/test

#### 3. Docker Build (Main branch only)
- Builds a multi-stage Docker image
- Pushes to Docker Hub
- Tags with branch name, semantic version, and latest

#### 4. Deploy to Staging (Develop branch)
- Runs after successful build and Docker build on `develop` branch
- Deploys to staging environment using Helm
- Uses `values-dev.yaml` configuration
- Waits for rollout completion
- Verifies pod health

#### 5. Deploy to Production (Main branch)
- Runs after successful build and Docker build on `main` branch
- Deploys to production environment using Helm
- Uses `values-prod.yaml` configuration
- Includes smoke tests after deployment
- Requires manual approval (GitHub environment protection)

## Local Development

### Using Docker Compose

Start the entire stack locally:

```bash
docker-compose up -d
```

This will:
- Start a PostgreSQL database
- Build and run the OrderService API
- Configure networking between services

Stop the stack:

```bash
docker-compose down
```

### Environment Variables

Update `docker-compose.yml` for your environment:
- `ASPNETCORE_ENVIRONMENT`: Set to Production/Development
- `ConnectionStrings__DefaultConnection`: Database connection string
- `JwtSettings__*`: JWT configuration

## GitHub Secrets

Configure these secrets in your GitHub repository settings:

| Secret | Description | Example |
|--------|-------------|---------|
| `DOCKERHUB_USERNAME` | Docker Hub username | `your-dockerhub-username` |
| `DOCKERHUB_TOKEN` | Docker Hub access token | (access token) |
| `KUBE_CONFIG_STAGING` | Base64 encoded kubeconfig for staging | (base64 kubeconfig) |
| `KUBE_CONFIG_PRODUCTION` | Base64 encoded kubeconfig for production | (base64 kubeconfig) |

### Creating Docker Hub Access Token

1. **Login to Docker Hub:**
   - Go to https://hub.docker.com/settings/security
   - Click "New Access Token"
   - Give it a name (e.g., "GitHub Actions CI/CD")
   - Copy the token (you won't see it again!)

2. **Add to GitHub:**
   - Go to repository Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `DOCKERHUB_USERNAME`, Value: your Docker Hub username
   - Name: `DOCKERHUB_TOKEN`, Value: the access token from step 1

### Setting up Kubernetes Secrets

1. **Encode your kubeconfig:**
   ```bash
   # Linux/Mac
   cat ~/.kube/config | base64 -w 0
   
   # Windows PowerShell
   [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((Get-Content ~/.kube/config -Raw)))
   ```

2. **Add to GitHub:**
   - Go to repository Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `KUBE_CONFIG_STAGING` or `KUBE_CONFIG_PRODUCTION`
   - Value: Paste the base64 encoded kubeconfig

3. **Set up Environment Protection (Recommended):**
   - Go to repository Settings → Environments
   - Create `staging` and `production` environments
   - Add required reviewers for production deployments
   - Add environment secrets if different from repository secrets

## Docker Image Management

### Pulling Images

```bash
# Pull the latest image
docker pull your-dockerhub-username/orderservice:latest

# Pull a specific tag
docker pull your-dockerhub-username/orderservice:main-abc1234
```

### Running Docker Container

```bash
docker run -d \
  -p 5000:5000 \
  -e ConnectionStrings__DefaultConnection="your-connection-string" \
  -e JwtSettings__SecretKey="your-secret-key" \
  your-dockerhub-username/orderservice:latest
```

## Health Checks

The Docker image includes a health check endpoint. The container will:
- Start reporting healthy after 5 seconds
- Check every 30 seconds
- Timeout after 3 seconds
- Retry up to 3 times before marking unhealthy

## Test Results

Test results are:
- Automatically uploaded as artifacts
- Reported in the GitHub Actions UI
- Available for download after each run

View coverage reports:
- Codecov dashboard: https://codecov.io

## Debugging Failed Builds

1. Check the GitHub Actions logs for the specific job
2. Review test output for failures
3. Check Docker build logs if Docker build fails
4. Verify all required secrets are configured

## Helm Deployment

The CI/CD pipeline is configured to deploy using Helm charts located in `../../helm/microservices`.

### Deployment Flow

1. **Staging Deployment** (develop branch):
   - Automatically deploys to staging environment
   - Uses `values-dev.yaml` configuration
   - Image tag: Git SHA for precise version tracking

2. **Production Deployment** (main branch):
   - Automatically deploys to production environment
   - Uses `values-prod.yaml` configuration
   - Requires manual approval (configure in GitHub Environments)
   - Includes smoke tests after deployment

### Manual Helm Deployment

You can also deploy manually using Helm:

```bash
# Navigate to helm charts directory
cd ../../helm/microservices

# Deploy to staging
helm upgrade --install microservices . \
  --namespace microservices \
  --create-namespace \
  --values values-dev.yaml \
  --set services.orderCsharp.image.repository=ghcr.io/your-org/ordermicroservice \
  --set services.orderCsharp.image.tag=latest

# Deploy to production
helm upgrade --install microservices . \
  --namespace microservices \
  --create-namespace \
  --values values-prod.yaml \
  --set services.orderCsharp.image.repository=ghcr.io/your-org/ordermicroservice \
  --set services.orderCsharp.image.tag=v1.0.0

# Check deployment status
kubectl get pods -n microservices -l app=order-csharp-service
kubectl logs -n microservices -l app=order-csharp-service --tail=50

# Rollback if needed
helm rollback microservices -n microservices
```

### Helm Chart Configuration

The OrderService is configured in the Helm chart with:
- Database connection to PostgreSQL
- JWT authentication settings
- Health check probes (liveness & readiness)
- Resource limits and requests
- Horizontal scaling support

See `../../helm/microservices/templates/services/order-csharp-service.yaml` for full configuration.

## Adding Custom Deployment

To customize deployment, you can modify the deploy jobs in `.github/workflows/ci-cd.yml`:

### Example: Additional Post-Deployment Steps

```yaml
  deploy-production:
    # ... existing steps ...
    
    - name: Run Database Migrations
      run: |
        kubectl exec -n microservices deployment/order-csharp-service -- \
          dotnet ef database update

    - name: Notify Team
      run: |
        curl -X POST ${{ secrets.SLACK_WEBHOOK }} \
          -d '{"text":"OrderService deployed to production!"}'
```

### Example: Blue-Green Deployment

```yaml
  deploy:
    needs: [build-and-test, build-docker]
    runs-on: ubuntu-latest
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Deploy to Kubernetes
      run: |
        kubectl set image deployment/orderservice \
          orderservice=ghcr.io/${{ github.repository }}:latest \
          --record
```

### Example: Docker Swarm

```yaml
  deploy:
    needs: [build-and-test, build-docker]
    runs-on: ubuntu-latest
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    
    steps:
    - name: Deploy to Docker Swarm
      run: |
        docker service update --image \
          ghcr.io/${{ github.repository }}:latest \
          orderservice
```

## Best Practices

1. **Branch Protection**: Require status checks to pass before merging
2. **Review Process**: Require code reviews for pull requests
3. **Secrets Management**: Never commit secrets; use GitHub Secrets
4. **Versioning**: Use semantic versioning for releases
5. **Monitoring**: Monitor deployed services for issues
6. **Rollback**: Keep previous versions available for quick rollback

## Troubleshooting

### Tests Failing Locally but Passing in CI

- Ensure local .NET SDK version matches CI (10.0.x)
- Clear local build cache: `dotnet clean`
- Restore dependencies: `dotnet restore`

### Docker Build Failures

- Ensure Dockerfile paths are correct relative to build context
- Verify all required files are included in the docker build context
- Check .dockerignore for unintended exclusions

### Deployment Failures

- Verify connection strings and secrets are correct
- Check infrastructure availability
- Review deployment logs for specific error messages
