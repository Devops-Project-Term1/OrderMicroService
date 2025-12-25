#!/bin/bash

# Script to build and publish OrderService Docker image to Docker Hub

# Configuration
DOCKER_USERNAME="${DOCKER_USERNAME:-Uteytithya}"  # Replace or set environment variable
IMAGE_NAME="orderservice"
VERSION="${VERSION:-latest}"

# Full image name
FULL_IMAGE_NAME="${DOCKER_USERNAME}/${IMAGE_NAME}:${VERSION}"

echo "======================================"
echo "Publishing OrderService to Docker Hub"
echo "======================================"
echo "Username: ${DOCKER_USERNAME}"
echo "Image: ${IMAGE_NAME}"
echo "Version: ${VERSION}"
echo "Full Image: ${FULL_IMAGE_NAME}"
echo "======================================"
echo ""

# Step 1: Build the Docker image
echo "Step 1: Building Docker image..."
cd OrderService
docker build -t ${IMAGE_NAME}:${VERSION} -f Dockerfile .

if [ $? -ne 0 ]; then
    echo "Error: Docker build failed!"
    exit 1
fi

echo "✓ Image built successfully"
echo ""

# Step 2: Tag the image for GHCR
echo "Step 2: Tagging image for GHCR..."
docker tag ${IMAGE_NAME}:${VERSION} ${FULL_IMAGE_NAME}

if [ $? -ne 0 ]; then
    echo "Error: Docker tag failed!"
    exit 1
fi

echo "✓ Image tagged successfully"
echo ""

# Step 3: Login to Docker Hub
echo "Step 3: Logging in to Docker Hub..."
echo ""

# Check if already logged in
if docker info 2>/dev/null | grep -q "Username"; then
    echo "✓ Already logged in to Docker Hub"
else
    echo "Please login with: docker login -u ${DOCKER_USERNAME}"
    read -p "Press Enter to continue after logging in..."
fi

echo ""

# Step 4: Push the image
echo "Step 4: Pushing image to Docker Hub..."
docker push ${FULL_IMAGE_NAME}

if [ $? -ne 0 ]; then
    echo "Error: Docker push failed!"
    echo "Make sure you're logged in with: docker login -u ${DOCKER_USERNAME}"
    exit 1
fi

echo ""
echo "======================================"
echo "✓ SUCCESS!"
echo "======================================"
echo "Image published to: ${FULL_IMAGE_NAME}"
echo ""
echo "To pull this image:"
echo "  docker pull ${FULL_IMAGE_NAME}"
echo ""
echo "To use in docker-compose or Kubernetes:"
echo "  image: ${FULL_IMAGE_NAME}"
echo "======================================"
