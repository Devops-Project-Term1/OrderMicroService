#!/usr/bin/env pwsh
# Configure Railway Environment Variables for Inter-Service Communication

Write-Host "🚂 Railway Inter-Service Configuration" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check if railway CLI is installed
try {
    railway --version | Out-Null
    Write-Host "✅ Railway CLI found" -ForegroundColor Green
} catch {
    Write-Host "❌ Railway CLI not found. Install it with:" -ForegroundColor Red
    Write-Host "   npm install -g @railway/cli"
    exit 1
}

Write-Host ""

# Check if logged in
try {
    railway whoami | Out-Null
    Write-Host "✅ Logged in to Railway" -ForegroundColor Green
} catch {
    Write-Host "❌ Not logged in to Railway. Login with:" -ForegroundColor Red
    Write-Host "   railway login"
    exit 1
}

Write-Host ""
Write-Host "This script will configure environment variables for inter-service communication" -ForegroundColor Yellow
Write-Host ""

# Select project
Write-Host "Select your Railway project ID (or press Enter to use current):" -ForegroundColor Cyan
$projectId = Read-Host

if ($projectId) {
    railway link $projectId
}

Write-Host ""
Write-Host "Choose deployment option:" -ForegroundColor Yellow
Write-Host "  1) Private networking (services in same project) - Recommended"
Write-Host "  2) Public URLs (services in different projects)"
$choice = Read-Host "Enter choice (1 or 2)"

Write-Host ""

if ($choice -eq "1") {
    # Private networking
    Write-Host "Configuring for private networking..." -ForegroundColor Cyan
    Write-Host ""
    
    $productUrl = "http://product-service.railway.internal:3000"
    $stockUrl = "http://stock-service.railway.internal:5000"
    
    Write-Host "Product Service URL: $productUrl" -ForegroundColor Gray
    Write-Host "Stock Service URL: $stockUrl" -ForegroundColor Gray
    
} elseif ($choice -eq "2") {
    # Public URLs
    Write-Host "Enter public URLs for your services:" -ForegroundColor Cyan
    Write-Host ""
    
    $productUrl = Read-Host "Product Service URL (e.g., https://product-service.up.railway.app)"
    $stockUrl = Read-Host "Stock Service URL (e.g., https://stock-service.up.railway.app)"
    
} else {
    Write-Host "Invalid choice" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Setting Railway environment variables..." -ForegroundColor Cyan
Write-Host ""

# Set environment variables
railway variables set "ServiceUrls__ProductService=$productUrl"
railway variables set "ServiceUrls__StockService=$stockUrl"

# Set other common variables
railway variables set "ASPNETCORE_ENVIRONMENT=Production"
railway variables set "ASPNETCORE_URLS=http://0.0.0.0:`$PORT"

Write-Host ""
Write-Host "✅ Environment variables configured!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Deploy your service: railway up"
Write-Host "2. Check logs: railway logs"
Write-Host "3. Test inter-service communication"
Write-Host ""
