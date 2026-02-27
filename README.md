# 🍔 Food Delivery Platform - Cloud-Native Backend API

![.NET](https://img.shields.io/badge/.NET_9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Azure](https://img.shields.io/badge/azure-%230072C6.svg?style=for-the-badge&logo=microsoftazure&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/EF_Core-512BD4?style=for-the-badge&logo=.net&logoColor=white)

A robust, cloud-hosted RESTful API built with **ASP.NET Core**, serving as the backbone for a modern food delivery application. It features secure data management, real-time order tracking, and enterprise-grade AI integration for business intelligence.

---

## 🔗 The "Twin-Star" Architecture

This project adopts a modern decoupled architecture. You are currently viewing the **Backend API** repository.

* 🖥️ **Frontend Client Portal (React/Vite):** [View Repository](https://github.com/Heng1007/food-delivery-client)
* 🌐 **Live Web Demo:** [Experience the App](https://food-delivery-client-62vj.vercel.app/login) *(Deployed on Vercel)*
* 📚 **Interactive API Docs:** [Swagger UI](https://heng-food-api-amc2aab4hdebekhg.southeastasia-01.azurewebsites.net/swagger/index.html) *(Deployed on Azure App Service)*

---

## ✨ Key Technical Highlights & Features

As a backend-focused developer, I tackled a mix of foundational architecture design and real-world cloud engineering challenges in this project:

* **🔥 C# Fundamentals & RESTful API Design:** Built a clean, standard REST API using **ASP.NET Core**. Utilized **DTOs (Data Transfer Objects)** to strictly separate internal database models from external API contracts. Configured the HTTP request pipeline using **Middleware** and implemented **Dependency Injection (DI)** to maintain loose coupling across controllers and services. Heavily relied on **Asynchronous Programming (`async/await`)** to ensure non-blocking database operations.
* **🔐 Authentication & Security:** Engineered a secure login system utilizing **JWT (JSON Web Tokens)** for stateless authentication and Role-Based Access Control (RBAC) to protect Admin-only endpoints. Implemented best practices for credential management using `.NET User Secrets` locally and **Azure Environment Variables** in production to completely isolate database connection strings and AI API keys.
* **🗄️ Relational Data Modeling (Azure SQL & EF Core):** Engineered a highly normalized database schema utilizing **Entity Framework Core (Code-First approach)**. Managed complex entity relationships across `Users`, `FoodItems`, `Orders`, and `OrderItems` (acting as a junction table to resolve many-to-many relationships). Wrote efficient **LINQ** queries to aggregate accurate business metrics, such as calculating total revenue by dynamically filtering out "Cancelled" orders.
* **☁️ Cloud-Native Deployment & Networking:** Successfully deployed the API to **Microsoft Azure App Service**. Tackled real-world networking issues by configuring strict **CORS (Cross-Origin Resource Sharing)** policies, ensuring the API securely accepts HTTP requests only from the authorized Vercel-hosted frontend domain.
* **🧠 AI-Driven Business Intelligence (Azure AI Language):** Integrated Azure Text Analytics to process customer textual feedback/notes, generating real-time sentiment scores. This transforms raw text inputs into actionable business intelligence displayed directly on the Admin Dashboard.

---

## 🏗️ System Architecture & Database

The system architecture follows a clean, decoupled design pattern, ensuring separation of concerns between API routing, business logic, and data access.

**Core Database Entities:**
* `User`: Handles Role-Based Access Control (RBAC), distinguishing between `Admin` and standard `Customer` privileges.
* `FoodItem`: Represents the restaurant's menu catalog, storing essential details like name, description, price, and image URLs.
* `Order`: Tracks the high-level order lifecycle (`Pending` -> `InProgress` -> `Delivered` / `Cancelled`) and aggregates the total transaction amount.
* `OrderItem`: Acts as a crucial **Junction Table** resolving the many-to-many relationship between `Order` and `FoodItem`. It captures the historical price and quantity at the exact time of purchase, ensuring data integrity even if menu prices change later.

---

## 🚀 Getting Started (Local Development)

To run this API locally, you will need the .NET 8.0 SDK and access to a SQL Server instance.

### 1. Clone the repository
```bash
git clone <YOUR_BACKEND_REPO_URL>
cd <YOUR_BACKEND_FOLDER_NAME>
```

### 2. Configure User Secrets (Database & AI)
Do not hardcode credentials. Initialize user secrets:
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_ID;Password=YOUR_PASSWORD;"
dotnet user-secrets set "LanguageService__Endpoint" "YOUR_AZURE_AI_ENDPOINT"
dotnet user-secrets set "LanguageService__Key" "YOUR_AZURE_AI_KEY"
```

### 3. Apply Database Migrations
```bash
dotnet ef database update
```

### 4. Run the Application
```bash
dotnet run
```
*Navigate to `https://localhost:<port>/swagger` to test the endpoints locally.*
