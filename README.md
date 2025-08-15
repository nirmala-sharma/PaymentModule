**PaymentModule**
****
**🌐 Project Overview**

This project presents a full-featured **payment gateway simulation**, built using modern architectural principles and best practices across both backend and frontend.

It includes a robust **Payment Module** designed with **event-driven architecture** to efficiently process transactions, seamlessly integrate with a **simulated third-party API**, and handle **asynchronous events** such as transaction status updates — all while ensuring reliability, scalability, and resilience.

**This Project Consists of:**
** **
**1. PaymentGatewayMockAPI** — A **mock third-party payment API** to simulate real-world payment processing.

**2. PaymentGatewayWebAPI** – Main backend API handling payment processing, idempotency, transaction logging, and event publishing.

**3. Event-Driven Architecture** — Utilizes **RabbitMQ** for **asynchronous** message handling between services.

**4. Authentication & Authorization** — Implements **secure user identity** management using **JWT-based access and refresh tokens**, ensuring safe and seamless authentication throughout the application.

**5. Logging** — Integrated **Serilog** for structured and detailed **application logs**.

**6. Global Exception Handling** — **Centralized error handling** for consistent API responses.

**7. Retry Policy** — Implements **automatic retries** using **Polly** to handle **transient failures**.

**8. Idempotency Support** — Prevents **duplicate payment processing** through idempotent request handling.

**9. PaymentGatewayApp (Angular)** — Frontend client built with Angular, supporting secure token management, form submission, and graceful error handling.

**10. Unit Testing** – Includes backend unit tests using **xUnit**, covering core services and controller logic to ensure **reliability**.

**11. CORS Support** – Enables secure **cross-origin request**s between frontend (Angular) and backend (.NET API).

**12. Rate Limiting** – Applies **IP-based request limits** using Fixed Window strategy to prevent **API overuse**. Allows 1 request per minute with 1 queued request, returning HTTP 429 when exceeded.

**13. Database Seed Service** – **Initializes** the database with default user records on **application startup**. Helps **automate setup** for **development, testing, or first-time deployments**.

**14. SignalR Integration** – Enables **real-time communication** between server and clients for instant payment status updates. Improves user experience by providing **live feedback** during payment processing without **page refreshes** or polling.

****
**Technology Used:**
.Net8.0
, Angular CLI: 14.2.13
, Node:16.16.0
, Typescript: 4.7.4
, npm 8.11.0 

****
**⚙️ How to Set Up This Solution :**

1. Clone the project repository to your local machine.

2. Restore all backend dependencies using your IDE or .NET CLI.

3. Update configuration files (appsettings.json) :

 - Database connection string

 - RabbitMQ credentials

 - Serilog logging settings

4. Build and run the .NET solution to automatically create the database and required tables.

5. Install frontend dependencies in the Angular project.

6. Build and run the Angular application
   
7. Access the application in your browser and begin testing.

   ****

   # 🐳 Dockerized Payment Module

This project is fully containerized using **Docker** and **Docker Compose**.  
Prebuilt images are available on **Docker Hub**, so you can run the entire stack without building locally.

---

## 📦 Architecture & Services

1. **PaymentGatewayMockAPI**  
   A mock third-party payment API to simulate real-world payment processing.  
   **Docker Hub:** (https://hub.docker.com/repository/docker/niru0102sharma/paymentmodulemockapiimage)

2. **PaymentGatewayWebAPI**  
   Main backend API handling payment processing, idempotency, transaction logging, and event publishing.  
   **Docker Hub:**(https://hub.docker.com/repository/docker/niru0102sharma/paymentmoduleserverimage)

3. **Client Application**  
   Frontend interface for interacting with the payment gateway system.  
   **Docker Hub:** (https://hub.docker.com/repository/docker/niru0102sharma/paymentmoduleclientapp)

4. **Event-Driven Architecture with RabbitMQ**  
   Utilizes RabbitMQ for asynchronous message handling between services.  
   Uses the official `rabbitmq:3-management` image from Docker Hub.

---

## 🖥️ Run the Stack with Docker Compose

Clone this repository and run:

```bash
docker compose -f docker-compose.yml -f docker-compose.hub.yml up -d





