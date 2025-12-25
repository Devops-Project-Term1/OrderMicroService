# OrderService API - PowerShell Commands
# Base URL - Update this to match your environment
$BaseUrl = "http://localhost:5247"

# JWT Token from Auth Service - Replace with your actual token
$Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlRlc3QgVXNlciIsImlzcyI6IkF1dGhTZXJ2aWNlIiwiYXVkIjoiT3JkZXJTZXJ2aWNlIiwiaWF0IjoxNTE2MjM5MDIyfQ.Wde4H-HMJ0_3R4niHdX7y4BPOxvFYlHtbjIKRhhYWK8"

# Trust self-signed certificates
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

# ============================================
# CREATE - POST /api/orders
# ============================================
Write-Host "=== CREATE Order ===" -ForegroundColor Green
$CreateBody = @{
    productId = "PROD-001"
    quantity = 5
    totalPrice = 99.99
} | ConvertTo-Json

$Response = Invoke-WebRequest -Uri "$BaseUrl/api/orders" `
  -Method POST `
  -Headers @{
    "Content-Type" = "application/json"
    "Authorization" = "Bearer $Token"
  } `
  -Body $CreateBody

Write-Host $Response.Content
Write-Host "`n`n"

# ============================================
# READ ALL - GET /api/orders
# ============================================
Write-Host "=== GET All Orders ===" -ForegroundColor Green
$Response = Invoke-WebRequest -Uri "$BaseUrl/api/orders" `
  -Method GET `
  -Headers @{
    "Authorization" = "Bearer $Token"
  }

Write-Host $Response.Content
Write-Host "`n`n"

# ============================================
# READ BY ID - GET /api/orders/{id}
# ============================================
Write-Host "=== GET Order by ID (ID: 1) ===" -ForegroundColor Green
$Response = Invoke-WebRequest -Uri "$BaseUrl/api/orders/1" `
  -Method GET `
  -Headers @{
    "Authorization" = "Bearer $Token"
  }

Write-Host $Response.Content
Write-Host "`n`n"

# ============================================
# DELETE - DELETE /api/orders/{id}
# ============================================
Write-Host "=== DELETE Order (ID: 1) ===" -ForegroundColor Green
$Response = Invoke-WebRequest -Uri "$BaseUrl/api/orders/1" `
  -Method DELETE `
  -Headers @{
    "Authorization" = "Bearer $Token"
  }

Write-Host "Delete successful - Status Code: $($Response.StatusCode)"
Write-Host "`n`n"

# ============================================
# Without Token (Should return 401 Unauthorized)
# ============================================
Write-Host "=== GET without Token (Should fail) ===" -ForegroundColor Red
try {
    $Response = Invoke-WebRequest -Uri "$BaseUrl/api/orders" -Method GET
} catch {
    Write-Host "Error (Expected): $($_.Exception.Response.StatusCode) - Unauthorized"
}

Write-Host "`n"
