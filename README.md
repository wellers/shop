# Shop Microservices
A containerised, distributed shopping platform built with .NET 8, demonstrating microservices architecture, event-driven communication, polyglot persistence, and Kubernetes-based deployment.

This project showcases production-style backend design patterns including API gateway routing, asynchronous messaging, and service isolation.

## Architecture Overview
The system is composed of independent microservices communicating via REST and message queues, deployed in containers and exposed via an Nginx gateway.

High-level flow:
1. Customers create baskets via the Basket service.
2. When a basket is purchased, a message is published to RabbitMQ.
3. The Booking service consumes the event and persists the booking.
4. The Booking service then publishes a booking completed event.
5. The Basket service consumes the event to clear the Redis basket.
6. The Mailout service consumes the event to simulate sending a confirmation email.
7. Product data is exposed via a GraphQL Catalog service backed by MongoDB.

## Microservices

### Basket Service
* .NET 8 REST API
* Redis-backed basket storage
* Publishes purchase events to RabbitMQ
* Consumes booking completed events to clear purchased baskets
* Demonstrates ephemeral, high-performance state handling

### Booking Service
* .NET 8 background consumer
* Subscribes to basket purchase events
* Persists bookings to Postgres
* Publishes booking completed events
* Demonstrates asynchronous, event-driven workflows

### Catalog Service
* GraphQL API
* MongoDB persistence
* Scheduled job to populate product data
* Demonstrates flexible querying and polyglot persistence

### Mailout Service
* .NET 8 background worker service
* Subscribes to booking completed events
* Simulates sending customer confirmation emails
* Demonstrates additional event-driven consumers within the architecture

### API Gateway (Kube-Proxy)
* Nginx reverse proxy
* Routes external traffic into the Kubernetes cluster
* Demonstrates gateway pattern in microservices architecture

### Internal Communication
* gRPC used for efficient internal service-to-service communication
* RabbitMQ used for asynchronous event propagation
* Multiple services consume events (Booking, Basket, Mailout)
* Clear separation between synchronous and asynchronous workflows

This reflects real-world distributed system design patterns.

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
* gRPC internal communication
* Polyglot persistence
* API gateway pattern
* Containerised services
* Kubernetes networking
* Background processing
* REST + GraphQL coexistence