 # Shop Microservices
A containerised, distributed shopping platform built with .NET 8, demonstrating microservices architecture, event-driven communication, polyglot persistence, and Kubernetes-based deployment.

This project showcases production-style backend design patterns including API gateway routing, asynchronous messaging, and service isolation.

## Architecture Overview
The system is composed of independent microservices communicating via REST and message queues, deployed in containers and exposed via an Nginx gateway.

High-level flow:
1. Customers create baskets via the Basket service.
2. When a basket is purchased, a message is published to RabbitMQ.
3. The Booking service consumes the event and persists the booking.
4. Product data is exposed via a GraphQL Catalog service backed by MongoDB.

## Microservices

### Basket Service
* .NET 8 REST API
* Redis-backed basket storage
* Publishes purchase events to RabbitMQ
* Demonstrates ephemeral, high-performance state handling

### Booking Service
* .NET 8 background consumer
* Subscribes to basket purchase events
* Persists bookings to Postgres
* Demonstrates asynchronous, event-driven workflows

### Catalog Service
* GraphQL API
* MongoDB persistence
* Scheduled job to populate product data
* Demonstrates flexible querying and polyglot persistence

### API Gateway (Kube-Proxy)
* Nginx reverse proxy
* Routes external traffic into the Kubernetes cluster
* Demonstrates gateway pattern in microservices architecture

## Shared Infrastructure
* RabbitMQ (message broker)
* Redis (basket storage)
* Postgres (booking persistence)
* MongoDB (catalog storage)
* pgAdmin (database management)
* Docker / Docker Compose
* Kubernetes deployment configuration

## Key Architectural Patterns Demonstrated
* Microservices decomposition
* Event-driven architecture
* Asynchronous messaging
* Polyglot persistence
* API gateway pattern
* Containerised services
* Kubernetes networking
* Background processing
* REST + GraphQL coexistence
