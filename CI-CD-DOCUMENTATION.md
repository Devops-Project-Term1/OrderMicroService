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
- Pushes to GitHub Container Registry (GHCR)
- Tags with branch name, semantic version, and latest

#### 4. Deploy (Main branch only)
- Runs after successful build and Docker build
- Placeholder for deployment commands

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
| `REGISTRY` | Container registry URL | `ghcr.io` |
| `GITHUB_TOKEN` | Auto-generated GitHub token | (auto) |

## Docker Image Management

### Pulling Images

```bash
# Pull the latest image
docker pull ghcr.io/your-org/OrderMicroService:latest

# Pull a specific tag
docker pull ghcr.io/your-org/OrderMicroService:main-abc1234
```

### Running Docker Container

```bash
docker run -d \
  -p 5000:5000 \
  -e ConnectionStrings__DefaultConnection="your-connection-string" \
  -e JwtSettings__SecretKey="your-secret-key" \
  ghcr.io/your-org/OrderMicroService:latest
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

## Adding Deployment

To add actual deployment, modify the `deploy` job in `.github/workflows/ci-cd.yml`:

### Example: Kubernetes Deployment

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
