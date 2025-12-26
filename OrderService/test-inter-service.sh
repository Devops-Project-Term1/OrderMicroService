#!/bin/bash
# Quick test commands for inter-service communication

# ==============================================================================
# Setup: Port forward the services (run in separate terminals or background)
# ==============================================================================
# kubectl port-forward -n microservices svc/order-service 8080:8080 &
# kubectl port-forward -n microservices svc/product-service 3000:3000 &
# kubectl port-forward -n microservices svc/stock-service 5000:5000 &

echo "Inter-Service Communication Test Commands"
echo "=========================================="
echo ""

# ==============================================================================
# 1. Get Products via Order Service (NEW FEATURE)
# ==============================================================================
echo "1. Get products for dropdown (via Order Service)"
echo "   This endpoint calls Product Service internally"
echo ""
echo "curl http://localhost:8080/api/orders/products"
curl -s http://localhost:8080/api/orders/products | jq '.'
echo ""
echo ""

# ==============================================================================
# 2. Check Stock Before Creating Order
# ==============================================================================
echo "2. Check current stock for product ID 1"
echo ""
echo "curl http://localhost:5000/stock/1"
STOCK_BEFORE=$(curl -s http://localhost:5000/stock/1)
echo "$STOCK_BEFORE" | jq '.'
QUANTITY_BEFORE=$(echo "$STOCK_BEFORE" | jq -r '.quantity')
echo ""
echo "Current stock: $QUANTITY_BEFORE units"
echo ""
echo ""

# ==============================================================================
# 3. Create Order (STOCK REDUCTION HAPPENS AUTOMATICALLY)
# ==============================================================================
echo "3. Create an order (this will automatically reduce stock)"
echo ""
echo "curl -X POST http://localhost:8080/api/orders \\"
echo "  -H 'Content-Type: application/json' \\"
echo "  -d '{\"productId\": 1, \"quantity\": 2, \"totalPrice\": 50.00, \"userId\": \"test-user\"}'"
echo ""

curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "productId": 1,
    "quantity": 2,
    "totalPrice": 50.00,
    "userId": "test-user"
  }' | jq '.'

echo ""
echo ""

# ==============================================================================
# 4. Verify Stock Was Reduced
# ==============================================================================
echo "4. Verify stock was reduced"
echo ""
sleep 1
echo "curl http://localhost:5000/stock/1"
STOCK_AFTER=$(curl -s http://localhost:5000/stock/1)
echo "$STOCK_AFTER" | jq '.'
QUANTITY_AFTER=$(echo "$STOCK_AFTER" | jq -r '.quantity')
echo ""
echo "Stock after: $QUANTITY_AFTER units"
echo "Reduced by: $((QUANTITY_BEFORE - QUANTITY_AFTER)) units"
echo ""
echo ""

# ==============================================================================
# 5. Test Error Handling: Insufficient Stock
# ==============================================================================
echo "5. Test error handling: Try to order more than available stock"
echo ""
echo "curl -X POST http://localhost:8080/api/orders \\"
echo "  -H 'Content-Type: application/json' \\"
echo "  -d '{\"productId\": 1, \"quantity\": 99999, ...}'"
echo ""

curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "productId": 1,
    "quantity": 99999,
    "totalPrice": 1000000.00,
    "userId": "test-user"
  }' | jq '.'

echo ""
echo ""

# ==============================================================================
# 6. Test Error Handling: Invalid Product
# ==============================================================================
echo "6. Test error handling: Try to order non-existent product"
echo ""
echo "curl -X POST http://localhost:8080/api/orders \\"
echo "  -H 'Content-Type: application/json' \\"
echo "  -d '{\"productId\": 99999, ...}'"
echo ""

curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "productId": 99999,
    "quantity": 1,
    "totalPrice": 10.00,
    "userId": "test-user"
  }' | jq '.'

echo ""
echo ""

# ==============================================================================
# 7. Get All Orders
# ==============================================================================
echo "7. Get all orders"
echo ""
echo "curl http://localhost:8080/api/orders"
curl -s http://localhost:8080/api/orders | jq '.'
echo ""
