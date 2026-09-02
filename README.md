# 💳 Payment Gateway Module

> A full-stack payment gateway simulation built with **event-driven architecture**, **real-time communication**, and **enterprise-level reliability patterns** — following the same design principles used in production payment systems.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![Angular](https://img.shields.io/badge/Angular-14-DD0031?style=flat&logo=angular)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3-FF6600?style=flat&logo=rabbitmq)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat&logo=docker)
![CI](https://img.shields.io/badge/CI-GitHub_Actions-2088FF?style=flat&logo=githubactions)
![xUnit](https://img.shields.io/badge/Tests-xUnit-512BD4?style=flat)

---

## 📌 Overview

This project simulates a real-world payment processing system with a mock third-party payment API, asynchronous event handling, real-time status updates, and secure authentication — containerized with Docker so the entire stack runs with a single command.

---

## 🏗️ System Architecture

```
┌─────────────────┐                    ┌──────────────────────┐
│  Angular Client │ ──── HTTP/SignalR ─▶│  PaymentGateway API  │
│   (Port 5002)   │                    │      (Port 5001)      │
└─────────────────┘                    └──────────┬────────────┘
                                                   │ RabbitMQ Events
                                       ┌───────────▼────────────┐
                                       │    Mock Payment API     │
                                       │      (Port 5005)        │
                                       └────────────────────────┘
```

---

## ⚡ Engineering Features

| Feature | Implementation |
|---|---|
| Async Event Processing | RabbitMQ message broker between services |
| Real-time Updates | SignalR WebSocket — live payment status without polling |
| Authentication | JWT access tokens + refresh token rotation |
| Fault Tolerance | Polly retry policies for transient failures |
| Duplicate Prevention | Idempotency key validation on payment requests |
| Structured Logging | Serilog with detailed request tracing |
| Rate Limiting | Fixed window strategy — HTTP 429 on exceeded limits (1 req/min) |
| Global Error Handling | Centralized exception middleware — consistent API responses |
| Database Seeding | Auto-initializes default records on first startup |
| CORS Support | Secure cross-origin configuration between Angular and API |
| Unit Testing | xUnit + Moq — controllers and core service logic |

---

## 🐳 Run With Docker (Recommended)

No local installations required — Docker handles everything.

**Prerequisites:** [Docker Desktop](https://www.docker.com/products/docker-desktop/)

```bash
git clone https://github.com/nirmala-sharma/PaymentModule.git
cd PaymentModule
docker compose up
```

| Service | URL |
|---|---|
| 🌐 Angular Frontend | http://localhost:5002 |
| ⚙️ Backend API | http://localhost:5001 |
| 🔌 Mock Payment API | http://localhost:5005 |
| 🐇 RabbitMQ Dashboard | http://localhost:15672 |

> RabbitMQ dashboard login: `guest / guest`

---

## 🔧 Run Without Docker

```bash
# 1. Clone the repo
git clone https://github.com/nirmala-sharma/PaymentModule.git

# 2. Update appsettings.json with your local config
#    - SQL Server connection string
#    - RabbitMQ host and credentials

# 3. Run backend — auto-creates database and seeds default user
dotnet restore
dotnet run

# 4. Run frontend
cd paymentgatewayapp.client
npm install
ng serve

# 5. Open browser at http://localhost:4200
```

---

## ⚙️ CI Pipeline

Every push to `master` automatically triggers a GitHub Actions pipeline:

```
Push to master
    → Restore NuGet + npm dependencies
    → Build .NET solution + Angular frontend
    → Run all xUnit unit tests
    → Build all Docker images
```

Failing builds are flagged immediately — the codebase is always in a buildable, testable state.

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 8, ASP.NET Core Web API |
| Frontend | Angular 14, TypeScript 4.7 |
| Messaging | RabbitMQ 3 |
| Real-time | SignalR |
| Database | SQL Server, Entity Framework Core |
| Authentication | JWT, BCrypt password hashing |
| Resilience | Polly retry policies |
| Logging | Serilog |
| Testing | xUnit, Moq |
| Containerization | Docker, Docker Compose |
| CI | GitHub Actions |

---

## 📦 Docker Hub Images

Pre-built images are available on Docker Hub:

- 🔗 [Backend API](https://hub.docker.com/repository/docker/niru0102sharma/paymentmoduleserverimage)
- 🔗 [Angular Client](https://hub.docker.com/repository/docker/niru0102sharma/paymentmoduleclientapp)
- 🔗 [Mock Payment API](https://hub.docker.com/repository/docker/niru0102sharma/paymentmodulemockapiimage)

---

## 📁 Project Structure

```
PaymentModule/
├── PaymentGatewayApp/
│   ├── PaymentGatewayApp.Server/    # .NET 8 Web API
│   └── paymentgatewayapp.client/    # Angular 14 frontend
├── PaymentGatewayMockAPI/           # Mock third-party payment API
├── PaymentAppTestProject/           # xUnit unit tests
├── docker-compose.yml               # Multi-container orchestration
└── .github/workflows/ci.yml         # GitHub Actions CI pipeline
```




