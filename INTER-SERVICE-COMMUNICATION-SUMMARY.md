# Inter-Service Communication - Implementation Summary

## ✅ Implemented Features

### 1. Product Dropdown in Order Service
- **New Endpoint**: `GET /api/orders/products`
- **Purpose**: Fetch available products from Product Service to display in order creation form
- **Implementation**: HTTP client calls Product Service internally

### 2. Automatic Stock Reduction
- **When**: Creating an order via `POST /api/orders`
- **Process**:
  1. Validates product exists (calls Product Service)
  2. Reduces stock quantity (calls Stock Service)
  3. Creates order in database
  4. Rolls back stock if order creation fails

## 📁 Files Created/Modified

### New Service Files
- ✅ `OrderService/Service/IProductService.cs` - Interface for Product Service client
- ✅ `OrderService/Service/ProductService.cs` - HTTP client implementation
- ✅ `OrderService/Service/IStockService.cs` - Interface for Stock Service client
- ✅ `OrderService/Service/StockService.cs` - HTTP client implementation with stock reduction
- ✅ `OrderService/Model/ProductDto.cs` - DTO for products

### Modified Files
- ✅ `OrderService/Service/IOrderService.cs` - Added GetAvailableProductsAsync()
- ✅ `OrderService/Service/OrderService.cs` - Updated CreateOrderAsync() with validation and stock reduction
- ✅ `OrderService/Controllers/OrderController.cs` - Added /products endpoint
- ✅ `OrderService/Program.cs` - Registered HTTP clients
- ✅ `OrderService/appsettings.json` - Added ServiceUrls configuration
- ✅ `OrderService/appsettings.Development.json` - Added localhost URLs
- ✅ `helm/microservices/values-local.yaml` - Added Kubernetes service URLs

### Test & Documentation Files
- ✅ `helm/test-inter-service.sh` - Bash test script
- ✅ `helm/test-inter-service.ps1` - PowerShell test script
- ✅ `helm/rebuild-order-service.sh` - Rebuild and deploy script (Bash)
- ✅ `helm/rebuild-order-service.ps1` - Rebuild and deploy script (PowerShell)
- ✅ `OrderService/test-inter-service.sh` - Quick curl commands (Bash)
- ✅ `OrderService/test-inter-service.ps1` - Quick API test (PowerShell)
- ✅ `INTER-SERVICE-COMMUNICATION-IMPLEMENTATION.md` - Complete documentation

## 🔧 Configuration

### Local Development
```json
"ServiceUrls": {
  "ProductService": "http://localhost:3000",
  "StockService": "http://localhost:5000"
}
```

### Kubernetes
```yaml
ServiceUrls__ProductService: http://product-service.microservices.svc.cluster.local:3000
ServiceUrls__StockService: http://stock-service.microservices.svc.cluster.local:5000
```

## 🚀 Deployment Steps

### 1. Rebuild Order Service
```bash
cd Order/OrderMicroService/OrderService
docker build -t zeins/orderservice:latest .
docker push zeins/orderservice:latest
```

### 2. Update Kubernetes
```bash
cd helm
helm upgrade microservices ./microservices -f ./microservices/values-local.yaml
kubectl rollout restart deployment/order-service -n microservices
```

### 3. Verify
```bash
kubectl get pods -n microservices
kubectl logs -n microservices deployment/order-service -f
```

## 📊 API Examples

### Get Products (for dropdown)
```bash
GET http://localhost:8080/api/orders/products
```

**Response:**
```json
[
  {
    "id": 1,
    "name": "Laptop",
    "price": 999.99,
    "description": "High-performance laptop"
  }
]
```

### Create Order (auto-reduces stock)
```bash
POST http://localhost:8080/api/orders
Content-Type: application/json

{
  "productId": 1,
  "quantity": 2,
  "totalPrice": 50.00,
  "userId": "user123"
}
```

**Success Response (201):**
```json
{
  "id": 1,
  "productId": 1,
  "quantity": 2,
  "totalPrice": 50.00,
  "userId": "user123",
  "orderDate": "2025-12-26T10:30:00Z"
}
```

**Error Response (400 - Insufficient Stock):**
```json
{
  "error": "Insufficient stock for product 1"
}
```

**Error Response (400 - Product Not Found):**
```json
{
  "error": "Product with ID 999 not found"
}
```

## 🔍 Testing

### Automated Test
```bash
# Bash
./helm/test-inter-service.sh

# PowerShell
.\helm\test-inter-service.ps1
```

The test will:
1. ✅ Fetch products via Order Service
2. ✅ Check stock before order
3. ✅ Create order
4. ✅ Verify stock was reduced
5. ✅ Test error handling

### Manual Test
```bash
# Port forward services
kubectl port-forward -n microservices svc/order-service 8080:8080 &
kubectl port-forward -n microservices svc/product-service 3000:3000 &
kubectl port-forward -n microservices svc/stock-service 5000:5000 &

# Run test commands
cd Order/OrderMicroService/OrderService
./test-inter-service.sh
```

## 📈 OpenTelemetry Tracing

All inter-service HTTP calls are automatically traced. View in Jaeger:

1. **Port forward Jaeger:**
   ```bash
   kubectl port-forward -n monitoring svc/jaeger 16686:16686
   ```

2. **Open:** http://localhost:16686

3. **Select service:** `order-service`

4. **Find traces** showing:
   - Order Service → Product Service (validate product)
   - Order Service → Stock Service (reduce stock)
   - Order Service → Database (create order)

## 🔄 Flow Diagram

```
Client Request (POST /api/orders)
        ↓
┌──────────────────────────┐
│   Order Service          │
│  (OrdersController)      │
└────────────┬─────────────┘
             │
             ├─[1]─→ Product Service (GET /products/{id})
             │       Validate product exists
             │       ← Product data or 404
             │
             ├─[2]─→ Stock Service (POST /stock/{id}/adjust)
             │       Reduce stock by quantity
             │       ← Success or 400 (insufficient)
             │
             ├─[3]─→ Database
             │       Save order
             │       ← Order saved
             │
             ↓
       Success Response (201)
```

## ⚠️ Error Handling

| Scenario | HTTP Status | Response |
|----------|-------------|----------|
| Product not found | 400 Bad Request | `{"error": "Product with ID X not found"}` |
| Insufficient stock | 400 Bad Request | `{"error": "Insufficient stock for product X"}` |
| Product Service down | 500 Internal Error | `{"error": "Failed to fetch products"}` |
| Stock Service down | 500 Internal Error | Order not created, error logged |
| DB failure | 500 Internal Error | Stock rolled back automatically |

## 📝 Notes

- **Rollback Logic**: If order creation fails after stock reduction, the stock is automatically added back
- **Timeout**: HTTP clients have 30-second timeout
- **Instrumentation**: All HTTP calls use OpenTelemetry for distributed tracing
- **Validation**: Product existence is validated before stock reduction
- **Thread Safety**: Stock adjustment uses database transactions in Stock Service

## 🎯 Next Steps

To use this feature in production:

1. **Rebuild and push** the Order Service image
2. **Update Helm values** with correct service URLs
3. **Deploy to Kubernetes**
4. **Test** using the provided scripts
5. **Monitor** traces in Jaeger

For detailed instructions, see [INTER-SERVICE-COMMUNICATION-IMPLEMENTATION.md](INTER-SERVICE-COMMUNICATION-IMPLEMENTATION.md)
