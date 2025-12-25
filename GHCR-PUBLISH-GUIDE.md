# Publishing OrderService to GitHub Container Registry (GHCR)

This guide explains how to build and publish the OrderService Docker image to GitHub Container Registry.

## Prerequisites

1. **Docker installed** on your system
2. **GitHub Personal Access Token (PAT)** with the following scopes:
   - `write:packages` - to push images
   - `read:packages` - to pull images
   - `delete:packages` (optional) - to delete images

### Creating a GitHub Personal Access Token

1. Go to GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Click "Generate new token (classic)"
3. Give it a descriptive name (e.g., "GHCR Access")
4. Select scopes: `write:packages`, `read:packages`
5. Click "Generate token"
6. **Copy and save the token** (you won't see it again!)

## Method 1: Using PowerShell Script (Windows)

```powershell
# Set your GitHub username
$env:GITHUB_USERNAME = "your-github-username"

# Login to GHCR (use your PAT as password)
docker login ghcr.io -u your-github-username

# Run the publish script
.\publish-to-ghcr.ps1

# Or specify parameters directly
.\publish-to-ghcr.ps1 -GitHubUsername "your-username" -Version "v1.0.0"
```

## Method 2: Using Bash Script (Linux/Mac/WSL)

```bash
# Set your GitHub username
export GITHUB_USERNAME="your-github-username"

# Login to GHCR
echo $GITHUB_TOKEN | docker login ghcr.io -u your-github-username --password-stdin

# Make script executable
chmod +x publish-to-ghcr.sh

# Run the publish script
./publish-to-ghcr.sh

# Or with custom version
VERSION=v1.0.0 ./publish-to-ghcr.sh
```

## Method 3: Manual Steps

### 1. Login to GHCR

```bash
# Interactive login
docker login ghcr.io -u your-github-username

# Or using environment variable
echo $GITHUB_TOKEN | docker login ghcr.io -u your-github-username --password-stdin
```

### 2. Build the Docker Image

```bash
cd OrderService
docker build -t orderservice:latest -f Dockerfile .
```

### 3. Tag the Image for GHCR

```bash
# Format: ghcr.io/USERNAME/IMAGE_NAME:TAG
docker tag orderservice:latest ghcr.io/your-github-username/orderservice:latest

# You can also tag with specific versions
docker tag orderservice:latest ghcr.io/your-github-username/orderservice:v1.0.0
```

### 4. Push to GHCR

```bash
docker push ghcr.io/your-github-username/orderservice:latest
docker push ghcr.io/your-github-username/orderservice:v1.0.0
```

## Verifying the Published Image

1. Go to your GitHub profile
2. Click on "Packages" tab
3. You should see `orderservice` listed
4. Click on it to manage visibility and settings

## Making the Package Public

By default, GHCR packages are private. To make it public:

1. Go to the package page on GitHub
2. Click "Package settings"
3. Scroll to "Danger Zone"
4. Click "Change visibility"
5. Select "Public"

## Using the Published Image

### Pull the image

```bash
# Public image (no login needed if public)
docker pull ghcr.io/your-github-username/orderservice:latest

# Private image (requires login)
docker login ghcr.io -u your-github-username
docker pull ghcr.io/your-github-username/orderservice:latest
```

### In docker-compose.yml

```yaml
services:
  orderservice:
    image: ghcr.io/your-github-username/orderservice:latest
    ports:
      - "5000:5000"
    environment:
      - ConnectionStrings__DefaultConnection=...
```

### In Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: orderservice
spec:
  containers:
  - name: orderservice
    image: ghcr.io/your-github-username/orderservice:latest
    ports:
    - containerPort: 5000
```

## Troubleshooting

### Authentication Failed

```bash
# Re-login with correct credentials
docker logout ghcr.io
docker login ghcr.io -u your-github-username
```

### Permission Denied

- Ensure your PAT has `write:packages` scope
- Verify the username is correct (lowercase)
- Check repository visibility settings

### Image Not Found

- Verify the image name format: `ghcr.io/username/image:tag`
- Username must be lowercase
- Check if package visibility is set correctly

## Environment Variables

You can set these environment variables for easier use:

```bash
# Bash/Linux
export GITHUB_USERNAME="your-github-username"
export GITHUB_TOKEN="your-personal-access-token"

# PowerShell
$env:GITHUB_USERNAME = "your-github-username"
$env:GITHUB_TOKEN = "your-personal-access-token"
```

## Continuous Integration (CI/CD)

For automated publishing via GitHub Actions, see the workflow in `.github/workflows/docker-publish.yml` (if available).

## Best Practices

1. **Use semantic versioning** for tags (v1.0.0, v1.1.0, etc.)
2. **Tag both specific version and latest**
3. **Keep images small** - use multi-stage builds (already implemented)
4. **Scan for vulnerabilities** before publishing
5. **Document breaking changes** in release notes
