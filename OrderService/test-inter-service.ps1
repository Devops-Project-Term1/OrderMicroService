#!/usr/bin/env pwsh
# Quick test commands for inter-service communication

# ==============================================================================
# Setup: Port forward the services (run in separate terminals or background)
# ==============================================================================
# kubectl port-forward -n microservices svc/order-service 8080:8080
# kubectl port-forward -n microservices svc/product-service 3000:3000
# kubectl port-forward -n microservices svc/stock-service 5000:5000

Write-Host "Inter-Service Communication Test Commands" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# ==============================================================================
# 1. Get Products via Order Service (NEW FEATURE)
# ==============================================================================
Write-Host "1. Get products for dropdown (via Order Service)" -ForegroundColor Yellow
Write-Host "   This endpoint calls Product Service internally"
Write-Host ""
Write-Host "GET http://localhost:8080/api/orders/products" -ForegroundColor Gray
$products = Invoke-RestMethod -Uri "http://localhost:8080/api/orders/products" -Method Get
$products | ConvertTo-Json
Write-Host ""

# ==============================================================================
# 2. Check Stock Before Creating Order
# ==============================================================================
Write-Host "2. Check current stock for product ID 1" -ForegroundColor Yellow
Write-Host ""
Write-Host "GET http://localhost:5000/stock/1" -ForegroundColor Gray
$stockBefore = Invoke-RestMethod -Uri "http://localhost:5000/stock/1" -Method Get
$stockBefore | ConvertTo-Json
$quantityBefore = $stockBefore.quantity
Write-Host ""
Write-Host "Current stock: $quantityBefore units" -ForegroundColor Green
Write-Host ""

# ==============================================================================
# 3. Create Order (STOCK REDUCTION HAPPENS AUTOMATICALLY)
# ==============================================================================
Write-Host "3. Create an order (this will automatically reduce stock)" -ForegroundColor Yellow
Write-Host ""
$orderData = @{
    productId = 1
    quantity = 2
    totalPrice = 50.00
    userId = "test-user"
}

Write-Host "POST http://localhost:8080/api/orders" -ForegroundColor Gray
$orderData | ConvertTo-Json
Write-Host ""

try {
    $orderResponse = Invoke-RestMethod -Uri "http://localhost:8080/api/orders" `
        -Method Post `
        -Body ($orderData | ConvertTo-Json) `
        -ContentType "application/json"
    
    Write-Host "✅ Order created successfully!" -ForegroundColor Green
    $orderResponse | ConvertTo-Json
} catch {
    Write-Host "❌ Order creation failed!" -ForegroundColor Red
    Write-Host $_.Exception.Message
}
Write-Host ""

# ==============================================================================
# 4. Verify Stock Was Reduced
# ==============================================================================
Write-Host "4. Verify stock was reduced" -ForegroundColor Yellow
Write-Host ""
Start-Sleep -Seconds 1
Write-Host "GET http://localhost:5000/stock/1" -ForegroundColor Gray
$stockAfter = Invoke-RestMethod -Uri "http://localhost:5000/stock/1" -Method Get
$stockAfter | ConvertTo-Json
$quantityAfter = $stockAfter.quantity
Write-Host ""
Write-Host "Stock after: $quantityAfter units" -ForegroundColor Green
Write-Host "Reduced by: $($quantityBefore - $quantityAfter) units" -ForegroundColor Green
Write-Host ""

# ==============================================================================
# 5. Test Error Handling: Insufficient Stock
# ==============================================================================
Write-Host "5. Test error handling: Try to order more than available stock" -ForegroundColor Yellow
Write-Host ""
$invalidOrder = @{
    productId = 1
    quantity = 99999
    totalPrice = 1000000.00
    userId = "test-user"
}

Write-Host "POST http://localhost:8080/api/orders (quantity: 99999)" -ForegroundColor Gray
try {
    $errorResponse = Invoke-RestMethod -Uri "http://localhost:8080/api/orders" `
        -Method Post `
        -Body ($invalidOrder | ConvertTo-Json) `
        -ContentType "application/json"
    $errorResponse | ConvertTo-Json
} catch {
    Write-Host "Expected error:" -ForegroundColor Yellow
    Write-Host $_.Exception.Message -ForegroundColor Red
}
Write-Host ""

# ==============================================================================
# 6. Test Error Handling: Invalid Product
# ==============================================================================
Write-Host "6. Test error handling: Try to order non-existent product" -ForegroundColor Yellow
Write-Host ""
$invalidProduct = @{
    productId = 99999
    quantity = 1
    totalPrice = 10.00
    userId = "test-user"
}

Write-Host "POST http://localhost:8080/api/orders (productId: 99999)" -ForegroundColor Gray
try {
    $errorResponse = Invoke-RestMethod -Uri "http://localhost:8080/api/orders" `
        -Method Post `
        -Body ($invalidProduct | ConvertTo-Json) `
        -ContentType "application/json"
    $errorResponse | ConvertTo-Json
} catch {
    Write-Host "Expected error:" -ForegroundColor Yellow
    Write-Host $_.Exception.Message -ForegroundColor Red
}
Write-Host ""

# ==============================================================================
# 7. Get All Orders
# ==============================================================================
Write-Host "7. Get all orders" -ForegroundColor Yellow
Write-Host ""
Write-Host "GET http://localhost:8080/api/orders" -ForegroundColor Gray
$allOrders = Invoke-RestMethod -Uri "http://localhost:8080/api/orders" -Method Get
$allOrders | ConvertTo-Json
Write-Host ""
