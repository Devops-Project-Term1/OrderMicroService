#!/bin/bash
# Configure Railway Environment Variables for Inter-Service Communication

echo "🚂 Railway Inter-Service Configuration"
echo "======================================"
echo ""

# Check if railway CLI is installed
if ! command -v railway &> /dev/null; then
    echo "❌ Railway CLI not found. Install it with:"
    echo "   npm install -g @railway/cli"
    exit 1
fi

echo "✅ Railway CLI found"
echo ""

# Check if logged in
if ! railway whoami &> /dev/null; then
    echo "❌ Not logged in to Railway. Login with:"
    echo "   railway login"
    exit 1
fi

echo "✅ Logged in to Railway"
echo ""

echo "This script will configure environment variables for inter-service communication"
echo ""

# Select project
echo "Select your Railway project ID (or press Enter to use current):"
read -r PROJECT_ID

if [ -n "$PROJECT_ID" ]; then
    railway link "$PROJECT_ID"
fi

echo ""
echo "Choose deployment option:"
echo "  1) Private networking (services in same project) - Recommended"
echo "  2) Public URLs (services in different projects)"
read -r -p "Enter choice (1 or 2): " CHOICE

echo ""

if [ "$CHOICE" = "1" ]; then
    # Private networking
    echo "Configuring for private networking..."
    echo ""
    
    PRODUCT_URL="http://product-service.railway.internal:3000"
    STOCK_URL="http://stock-service.railway.internal:5000"
    
    echo "Product Service URL: $PRODUCT_URL"
    echo "Stock Service URL: $STOCK_URL"
    
elif [ "$CHOICE" = "2" ]; then
    # Public URLs
    echo "Enter public URLs for your services:"
    echo ""
    
    read -r -p "Product Service URL (e.g., https://product-service.up.railway.app): " PRODUCT_URL
    read -r -p "Stock Service URL (e.g., https://stock-service.up.railway.app): " STOCK_URL
    
else
    echo "Invalid choice"
    exit 1
fi

echo ""
echo "Setting Railway environment variables..."
echo ""

# Set environment variables
railway variables set "ServiceUrls__ProductService=$PRODUCT_URL"
railway variables set "ServiceUrls__StockService=$STOCK_URL"

# Set other common variables
railway variables set "ASPNETCORE_ENVIRONMENT=Production"
railway variables set "ASPNETCORE_URLS=http://0.0.0.0:\$PORT"

echo ""
echo "✅ Environment variables configured!"
echo ""
echo "Next steps:"
echo "1. Deploy your service: railway up"
echo "2. Check logs: railway logs"
echo "3. Test inter-service communication"
echo ""
