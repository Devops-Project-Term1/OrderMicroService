#!/bin/bash

# OrderService API - CURL Commands
# Base URL - Update this to match your environment
BASE_URL="http://localhost:5247" 

# JWT Token from Auth Service - Replace with your actual token
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30"

# ============================================
# CREATE - POST /api/orders
# ============================================
echo "=== CREATE Order ==="
curl -i -X POST "$BASE_URL/api/orders" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "productId": "PROD-001",
    "quantity": 5,
    "totalPrice": 99.99
  }' 2>&1

echo -e "\n\n"

# ============================================
# READ ALL - GET /api/orders
# ============================================
echo "=== GET All Orders ==="
curl -i -X GET "$BASE_URL/api/orders" \
  -H "Authorization: Bearer $TOKEN" 2>&1

echo -e "\n\n"

# ============================================
# READ BY ID - GET /api/orders/{id}
# ============================================
echo "=== GET Order by ID (ID: 1) ==="
curl -i -X GET "$BASE_URL/api/orders/1" \
  -H "Authorization: Bearer $TOKEN" 2>&1

echo -e "\n\n"

# ============================================
# DELETE - DELETE /api/orders/{id}
# ============================================
echo "=== DELETE Order (ID: 1) ==="
curl -i -X DELETE "$BASE_URL/api/orders/1" \
  -H "Authorization: Bearer $TOKEN" 2>&1

echo -e "\n\n"

# ============================================
# Without Token (Should return 401 Unauthorized)
# ============================================
echo "=== GET without Token (Should fail) ==="
curl -i -X GET "$BASE_URL/api/orders" 2>&1

echo -e "\n"
