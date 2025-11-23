# UserRoleAuth API -- ASP.NET Core 8 (Identity + EF Core + JWT + Roles + Localization)

A complete REST API demonstrating **User Registration**, **Login**,
**JWT Authentication**, **Role Management**, **Identity + EF Core**, and
**Multilingual API responses** using ASP.NET Core 8.

## 🚀 Features

-   ASP.NET Core 8 Web API
-   Authentication using **JWT Bearer Tokens**
-   Authorization using **Roles (Admin, etc.)**
-   ASP.NET Core **Identity + EF Core**
-   SQL Server Database
-   Auto-seed **Admin user + Admin role**
-   Full role & user management
-   Multilingual API responses (English/Hindi/Marathi)
-   Swagger UI with JWT Authentication

## 🛠 Prerequisites

Install: - .NET 8 SDK - SQL Server - Visual Studio or VS Code - Postman
/ Swagger (for testing)

## 🔧 Setup Instructions

### 1. Clone the Repository

``` sh
git clone https://github.com/<your-username>/<repo>.git
cd <repo>
```

### 2. Update appsettings.json

#### Connection String

``` json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=UserRoleAuthDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

#### JWT Settings

``` json
"Jwt": {
  "Key": "ThisIsMySuperSecureJwtKey123456789000!",
  "Issuer": "UserRoleAuthApi",
  "Audience": "UserRoleAuthApiUsers",
  "AccessTokenMinutes": "120",
  "RefreshTokenDays": "7"
}
```

### 3. Apply EF & Identity Migrations

``` sh
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Run the API

``` sh
dotnet run
```

API URL:

    https://localhost:7201

## 👑 Admin Auto-Seeding

Default Admin: - **Username:** admin - **Email:** admin@example.com -
**Password:** Admin@123 - **Role:** Admin

## 📘 API Testing Guide

### 1. Open Swagger

    https://localhost:7201/swagger

### 2. Login as Admin

POST → `/api/Auth/login`

``` json
{
  "userName": "admin",
  "password": "Admin@123"
}
```

### 3. Authorize in Swagger

    Bearer <token>

### ✔ Test All ADMIN APIs

#### Create Role

POST → `/api/Role`

``` json
{
  "name": "Manager",
  "description": "Manager role"
}
```

#### Create User

POST → `/api/User/register`

``` json
{
  "userName": "john",
  "email": "john@example.com",
  "password": "John@123",
  "displayName": "John Roy"
}
```

#### Assign Role

POST → `/api/User/assign-role`

``` json
{
  "userName": "john",
  "roleName": "Manager"
}
```

#### List Users

GET → `/api/User`

## 🌐 Multilingual Testing

Use header: - `Accept-Language: hi` (Hindi) 


## 🗄️ Verify Tables in SQL Server

``` sql
SELECT * FROM AspNetUsers;
SELECT * FROM AspNetRoles;
SELECT * FROM AspNetUserRoles;
```

## 🎯 Summary

This README includes: - Full setup (EF + Identity + SQL + JWT) - How to
run the API - Testing all admin/user APIs - Curl commands - DB
verification steps - Localization usage - Seed admin details

