---
name: kubernetes-ops
description: >
  Use when designing, deploying, or troubleshooting Kubernetes clusters and workloads.
  Covers cluster planning, Day-0 vs Day-1 decisions, networking, pod lifecycle,
  and kubectl troubleshooting. Trigger on terms like "Kubernetes", "K8s", "cluster",
  "pod", "deployment", "service", "ingress", "kubectl", "helm", "namespace",
  "container", or "orchestration".
---

# Kubernetes Operations

Guidance for Kubernetes cluster configuration, workload deployment, and troubleshooting.

Adapted from [microsoft/azure-skills/azure-kubernetes](https://github.com/microsoft/azure-skills).

## Day-0 Decisions (Hard to Change Later)

Make these decisions carefully during initial cluster setup:
- **Networking model** — CNI plugin (Calico, Cilium, Azure CNI), pod CIDR, service CIDR.
- **API server configuration** — Authentication/authorization modes, admission controllers.
- **Node pool architecture** — OS, VM sizes, auto-scaling boundaries, taints/tolerations.
- **Control plane access** — Private cluster, authorized IP ranges.

## Day-1 Features (Enable Post-Creation)

These can be added or adjusted later:
- **Monitoring** — Prometheus, Grafana, Azure Monitor.
- **Service mesh** — Linkerd, Istio for mTLS and traffic management.
- **Ingress controllers** — NGINX, Traefik, Application Gateway.
- **Policies** — OPA/Gatekeeper, NetworkPolicies, Pod Security Standards.

## Microservices Deployment Patterns

- One `Deployment` + `Service` per microservice.
- Use `HorizontalPodAutoscaler` (HPA) based on CPU/memory or custom metrics.
- Use `PodDisruptionBudget` to ensure availability during rolling updates.
- Run database migrations as `Job` resources before deploying application updates.
- Use `ConfigMap` for configuration and `Secret` for sensitive data (never commit secrets).

## Health Checks

- **Liveness probe** — `/health/live` — kubelet restarts the container if this fails.
- **Readiness probe** — `/health/ready` — traffic is routed only when this passes.
- **Startup probe** — `/health/startup` — disables liveness/readiness until slow-starting apps are ready.

## Troubleshooting Commands

```bash
# Check pod status and events
kubectl describe pod <pod-name> -n <namespace>

# View logs
kubectl logs <pod-name> -n <namespace> --previous  # previous container instance

# Execute into container
kubectl exec -it <pod-name> -n <namespace> -- /bin/sh

# Check resource usage
kubectl top pod -n <namespace>

# Check node status
kubectl get nodes -o wide
kubectl describe node <node-name>
```

## Service Mesh (mTLS)

- Deploy Linkerd or Istio for automatic mTLS between services.
- Gateway is the only externally reachable ingress.
- All internal service-to-service traffic goes through the mesh.
- NetworkPolicies default-deny; allow only mesh and gateway traffic.

## SaaS Platform Alignment

- This skill complements `saas-infrastructure` for K8s-specific operational details.
- Use it when writing Helm charts, Kustomize overlays, or raw YAML manifests.
- Ensure health checks align with the three-endpoint strategy in `saas-infrastructure`.
- Migrations run as Jobs; never use `MigrateAsync()` in application startup code.
