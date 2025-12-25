# Login and get token
$loginResponse = Invoke-RestMethod -Uri "http://localhost:3000/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body '{"email":"admin@orderservice.com","password":"Admin@123"}'

$token = $loginResponse.data.token
Write-Host "Token obtained:" $token.Substring(0, 50) "..."

# Create an order
Write-Host "`n=== Creating Order ===" -ForegroundColor Green
try {
    $createResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/orders" `
        -Method POST `
        -Headers @{
            "Authorization" = "Bearer $token"
            "Content-Type" = "application/json"
        } `
        -Body '{"productId":101,"quantity":5,"totalPrice":250.50}'
    
    Write-Host "Order created successfully:"
    $createResponse | ConvertTo-Json
} catch {
    Write-Host "Error creating order: $_" -ForegroundColor Red
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)"
}

# Get all orders
Write-Host "`n=== Getting All Orders ===" -ForegroundColor Green
try {
    $getResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/orders" `
        -Method GET `
        -Headers @{
            "Authorization" = "Bearer $token"
        }
    
    Write-Host "Orders retrieved successfully:"
    $getResponse | ConvertTo-Json
} catch {
    Write-Host "Error getting orders: $_" -ForegroundColor Red
}
