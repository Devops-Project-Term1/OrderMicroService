# Helm Deployment Guide for OrderService

## Overview

This guide explains how to deploy the OrderService using Helm, both through the CI/CD pipeline and manually.

## Prerequisites

- Kubernetes cluster (local or cloud)
- `kubectl` configured with cluster access
- `helm` v3.x installed
- Docker images pushed to Docker Hub

## Automated Deployment (CI/CD)

### Staging Deployment

Push to the `develop` branch:

```bash
git checkout develop
git add .
git commit -m "Deploy to staging"
git push origin develop
```

The pipeline will:
1. Run tests
2. Build Docker image
3. Push to GHCR with SHA tag
4. Deploy to staging using `values-dev.yaml`

### Production Deployment

Push to the `main` branch:

```bash
git checkout main
git merge develop
git push origin main
```

The pipeline will:
1. Run tests
2. Build Docker image
3. Push to GHCR with SHA and `latest` tags
4. Wait for manual approval (if configured)
5. Deploy to production using `values-prod.yaml`
6. Run smoke tests

## Manual Deployment

### 1. Build and Push Docker Image

```bash
# Build the image
cd OrderService
docker build -t your-dockerhub-username/orderservice:v1.0.0 .

# Login to Docker Hub
docker login

# Push to registry
docker push your-dockerhub-username/orderservice:v1.0.0
```

### 2. Deploy with Helm

```bash
# Navigate to helm charts
cd ../../helm/microservices

# Install or upgrade the release
helm upgrade --install microservices . \
  --namespace microservices \
  --create-namespace \
  --values values.yaml \
  --set services.orderCsharp.image.repository=your-dockerhub-username/orderservice \
  --set services.orderCsharp.image.tag=v1.0.0 \
  --wait
```

### 3. Verify Deployment

```bash
# Check pod status
kubectl get pods -n microservices -l app=order-csharp-service

# Check service
kubectl get svc -n microservices order-csharp-service

# View logs
kubectl logs -n microservices -l app=order-csharp-service --tail=100 -f

# Check deployment status
kubectl rollout status deployment/order-csharp-service -n microservices
```

## Environment-Specific Deployments

### Development Environment

```bash
helm upgrade --install microservices . \
  --namespace microservices-dev \
  --create-namespace \
  --values values-dev.yaml \
  --set services.orderCsharp.image.repository=your-dockerhub-username/orderservice \
  --set services.orderCsharp.image.tag=develop
```

### Production Environment

```bash
helm upgrade --install microservices . \
  --namespace microservices-prod \
  --create-namespace \
  --values values-prod.yaml \
  --set services.orderCsharp.image.repository=your-dockerhub-username/orderservice \
  --set services.orderCsharp.image.tag=v1.0.0
```

## Configuration Override

You can override any value in the Helm chart:

```bash
helm upgrade --install microservices . \
  --namespace microservices \
  --values values.yaml \
  --set services.orderCsharp.replicas=3 \
  --set services.orderCsharp.resources.limits.memory=2Gi \
  --set databases.orderCsharp.password=new-secure-password
```

Or create a custom values file:

```yaml
# custom-values.yaml
services:
  orderCsharp:
    replicas: 3
    image:
      repository: your-dockerhub-username/orderservice
      tag: v1.2.0
    resources:
      limits:
        memory: 2Gi
        cpu: 2000m
```

```bash
helm upgrade --install microservices . \
  --namespace microservices \
  --values values.yaml \
  --values custom-values.yaml
```

## Rollback

### Rollback to Previous Release

```bash
# List release history
helm history microservices -n microservices

# Rollback to previous version
helm rollback microservices -n microservices

# Rollback to specific revision
helm rollback microservices 3 -n microservices
```

### Rollback with Kubectl

```bash
# Undo the last deployment
kubectl rollout undo deployment/order-csharp-service -n microservices

# Rollback to specific revision
kubectl rollout undo deployment/order-csharp-service --to-revision=2 -n microservices
```

## Troubleshooting

### Check Helm Release Status

```bash
helm status microservices -n microservices
helm get values microservices -n microservices
helm get manifest microservices -n microservices
```

### Pod Not Starting

```bash
# Describe pod
kubectl describe pod -n microservices -l app=order-csharp-service

# Check events
kubectl get events -n microservices --sort-by='.lastTimestamp'

# Check logs
kubectl logs -n microservices -l app=order-csharp-service --previous
```

### Database Connection Issues

```bash
# Check database pod
kubectl get pods -n microservices -l app=postgres-order-csharp

# Test connection from OrderService pod
kubectl exec -n microservices deployment/order-csharp-service -- \
  psql -h postgres-order-csharp -U orderuser -d orderdb -c "SELECT 1"
```

### Service Not Accessible

```bash
# Check service endpoints
kubectl get endpoints -n microservices order-csharp-service

# Port forward to test locally
kubectl port-forward -n microservices svc/order-csharp-service 8080:8080

# Test the service
curl http://localhost:8080/health
```

## Uninstall

### Remove Helm Release

```bash
# Uninstall but keep history
helm uninstall microservices -n microservices

# Delete namespace (removes everything)
kubectl delete namespace microservices
```

## Advanced: CI/CD Setup

### Required GitHub Secrets

1. **DOCKERHUB_USERNAME** - Your Docker Hub username
2. **DOCKERHUB_TOKEN** - Docker Hub access token (create at hub.docker.com/settings/security)
3. **KUBE_CONFIG_STAGING** - Base64 encoded kubeconfig for staging cluster
4. **KUBE_CONFIG_PRODUCTION** - Base64 encoded kubeconfig for production cluster

### Encode Kubeconfig

```bash
# Linux/Mac
cat ~/.kube/config | base64 -w 0

# Windows PowerShell
[Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((Get-Content ~/.kube/config -Raw)))
```

### GitHub Environment Protection

1. Go to repository **Settings → Environments**
2. Create `staging` and `production` environments
3. For production:
   - Add required reviewers
   - Add deployment branch rules (only `main`)
   - Set deployment timeout

## Monitoring Deployments

### Watch Deployment Progress

```bash
# Watch pods
kubectl get pods -n microservices -l app=order-csharp-service -w

# Watch deployment
kubectl rollout status deployment/order-csharp-service -n microservices --watch
```

### View Deployment History

```bash
# Helm history
helm history microservices -n microservices

# Kubernetes rollout history
kubectl rollout history deployment/order-csharp-service -n microservices
```

## Best Practices

1. **Use Semantic Versioning** - Tag images with proper versions (v1.0.0, v1.1.0)
2. **Always Test in Staging First** - Never deploy directly to production
3. **Keep Helm Values DRY** - Use values-dev.yaml and values-prod.yaml for environment-specific configs
4. **Monitor Deployments** - Watch logs and metrics during rollout
5. **Have a Rollback Plan** - Know how to quickly rollback if issues occur
6. **Use Health Checks** - Ensure liveness and readiness probes are configured
7. **Set Resource Limits** - Prevent resource exhaustion
8. **Backup Databases** - Before major deployments, backup your data

## Related Documentation

- [CI/CD Documentation](CI-CD-DOCUMENTATION.md)
- [GHCR Publishing Guide](GHCR-PUBLISH-GUIDE.md)
- [Helm Charts](../../helm/microservices/README.md)
- [Helm Quick Start](../../helm/HELM-QUICKSTART.md)
