# Quick Start: Inter-Service Communication

## 🚀 Deploy the New Features

### Option 1: Automated Script (Recommended)
```bash
# Bash
cd helm
chmod +x rebuild-order-service.sh
./rebuild-order-service.sh

# PowerShell
cd helm
.\rebuild-order-service.ps1
```

### Option 2: Manual Steps
```bash
# 1. Build Docker image
cd Order/OrderMicroService/OrderService
docker build -t zeins/orderservice:latest .

# 2. Push to Docker Hub (optional for local testing)
docker push zeins/orderservice:latest

# 3. Update Kubernetes
cd ../../../helm
helm upgrade microservices ./microservices -f ./microservices/values-local.yaml

# 4. Restart pod
kubectl rollout restart deployment/order-service -n microservices
kubectl rollout status deployment/order-service -n microservices
```

## ✅ Verify Deployment

```bash
# Check pod status
kubectl get pods -n microservices

# Expected output:
# NAME                              READY   STATUS    RESTARTS
# order-service-xxx                 1/1     Running   0
# product-service-xxx               1/1     Running   0
# stock-service-xxx                 1/1     Running   0

# Check logs
kubectl logs -n microservices deployment/order-service -f
```

## 🧪 Test the Features

### Quick Test
```bash
# Bash
cd helm
./test-inter-service.sh

# PowerShell
cd helm
.\test-inter-service.ps1
```

### Manual Test
```bash
# 1. Port forward services
kubectl port-forward -n microservices svc/order-service 8080:8080 &
kubectl port-forward -n microservices svc/product-service 3000:3000 &
kubectl port-forward -n microservices svc/stock-service 5000:5000 &

# 2. Get products (for dropdown)
curl http://localhost:8080/api/orders/products

# 3. Check stock
curl http://localhost:5000/stock/1

# 4. Create order (stock will reduce automatically)
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "productId": 1,
    "quantity": 2,
    "totalPrice": 50.00,
    "userId": "test-user"
  }'

# 5. Verify stock was reduced
curl http://localhost:5000/stock/1
```

## 📊 View Distributed Traces

```bash
# 1. Port forward Jaeger
kubectl port-forward -n monitoring svc/jaeger 16686:16686

# 2. Open browser
http://localhost:16686

# 3. Select service: order-service
# 4. Click "Find Traces"
# 5. Click on a trace to see:
#    - Order Service → Product Service
#    - Order Service → Stock Service
#    - Complete request timeline
```

## 🔧 Troubleshooting

### Pod not starting?
```bash
kubectl describe pod -n microservices -l app=order-service
kubectl logs -n microservices deployment/order-service --previous
```

### Can't reach other services?
```bash
# Test from Order pod
kubectl exec -n microservices deployment/order-service -- \
  curl http://product-service:3000

kubectl exec -n microservices deployment/order-service -- \
  curl http://stock-service:5000
```

### Stock not reducing?
```bash
# Check Order Service logs
kubectl logs -n microservices deployment/order-service | grep -i stock

# Check Stock Service logs
kubectl logs -n microservices deployment/stock-service | grep -i adjust
```

## 📝 New Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/orders/products` | Get all products (for dropdown) |
| POST | `/api/orders` | Create order (now validates & reduces stock) |
| GET | `/api/orders` | Get all orders (unchanged) |
| GET | `/api/orders/{id}` | Get order by ID (unchanged) |

## 🎯 What Changed?

**Order Service now:**
- ✅ Calls Product Service to fetch products
- ✅ Validates product exists before creating order
- ✅ Automatically reduces stock when order is created
- ✅ Rolls back stock if order creation fails
- ✅ Returns proper error messages

**Features:**
- ✅ Products dropdown endpoint
- ✅ Stock reduction on order creation
- ✅ Error handling (product not found, insufficient stock)
- ✅ Distributed tracing with Jaeger
- ✅ Automatic rollback on failure

## 📚 Documentation

- **Summary**: [INTER-SERVICE-COMMUNICATION-SUMMARY.md](INTER-SERVICE-COMMUNICATION-SUMMARY.md)
- **Full Guide**: [INTER-SERVICE-COMMUNICATION-IMPLEMENTATION.md](../INTER-SERVICE-COMMUNICATION-IMPLEMENTATION.md)
- **Test Scripts**: `helm/test-inter-service.sh` or `.ps1`
