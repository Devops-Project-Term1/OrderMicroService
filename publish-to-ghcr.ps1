# PowerShell script to build and publish OrderService Docker image to Docker Hub

param(
    [string]$DockerUsername = "Uteytithya",
    [string]$ImageName = "orderservice",
    [string]$Version = "latest"
)

# Validate Docker username
if ([string]::IsNullOrEmpty($DockerUsername)) {
    Write-Host "Error: Docker Hub username is required!" -ForegroundColor Red
    Write-Host "Usage: .\publish-to-ghcr.ps1 -DockerUsername 'your-username'" -ForegroundColor Yellow
    exit 1
}

$FullImageName = "$($DockerUsername.ToLower())/$ImageName`:$Version"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Publishing OrderService to Docker Hub" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Username: $DockerUsername"
Write-Host "Image: $ImageName"
Write-Host "Version: $Version"
Write-Host "Full Image: $FullImageName"
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build the Docker image
Write-Host "Step 1: Building Docker image..." -ForegroundColor Yellow
Set-Location OrderService

docker build -t "${ImageName}:${Version}" -f Dockerfile .

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Docker build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Image built successfully" -ForegroundColor Green
Write-Host ""

# Step 2: Tag the image for GHCR
Write-Host "Step 2: Tagging image for GHCR..." -ForegroundColor Yellow
docker tag "${ImageName}:${Version}" $FullImageName

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Docker tag failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Image tagged successfully" -ForegroundColor Green
Write-Host ""

# Step 3: Login to GHCR
Write-Host "Step 3:Docker Hub
Write-Host "Step 3: Logging in to Docker Hub..." -ForegroundColor Yellow
Write-Host ""
Write-Host "To login, run: docker login -u $DockerUsername" -ForegroundColor Cyan

$continue = Read-Host "Have you logged in? (y/n)"
if ($continue -ne 'y') {
    Write-Host "Please login first, then run this script again." -ForegroundColor Yellow
    exit 0
}

Write-Host ""

# Step 4: Push the image
Write-Host "Step 4: Pushing image to GHCR..." -ForegroundColor Yellow
docker push $FullImageName

if ($LASTEXITCODE -ne 0) {Docker Hub..." -ForegroundColor Yellow
docker push $FullImageName

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Docker push failed!" -ForegroundColor Red
    Write-Host "Make sure you're logged in with: docker login -u $Docker
Write-Host ""
Write-Host "======================================" -ForegroundColor Green
Write-Host "✓ SUCCESS!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host "Image published to: $FullImageName" -ForegroundColor Cyan
Write-Host ""
Write-Host "To pull this image:" -ForegroundColor Yellow
Write-Host "  docker pull $FullImageName" -ForegroundColor White
Write-Host ""
Write-Host "To use in docker-compose or Kubernetes:" -ForegroundColor Yellow
Write-Host "  image: $FullImageName" -ForegroundColor White
Write-Host "======================================" -ForegroundColor Green
