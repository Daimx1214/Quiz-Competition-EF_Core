# Quiz Competition System

A backend-focused **Quiz Competition System** built with **ASP.NET Core
Web API**, **Entity Framework Core**, and **SQL Server**. The project
provides RESTful APIs for managing subjects, tests, questions,
participants, and quiz-related data through a structured database-driven
architecture.

## Overview

The Quiz Competition System is designed to manage quiz competitions
through a clean Web API architecture. It uses Entity Framework Core for
database access and migrations, SQL Server as the database provider, and
Swagger/OpenAPI for API exploration and testing.

The repository follows a controller-based ASP.NET Core structure with
separate models, DTOs, database context, and migrations.

## Features

-   Create and manage quiz subjects
-   Create and manage tests/quizzes
-   Associate subjects with tests
-   Manage quiz questions and related data
-   Retrieve test and subject information through REST APIs
-   Dashboard endpoints for quiz-related data and statistics
-   Entity Framework Core database integration
-   SQL Server database support
-   Code-first migrations
-   DTO-based request handling
-   Asynchronous API operations
-   Swagger/OpenAPI documentation
-   HTTPS redirection and API authorization middleware

## Tech Stack

  Technology              Purpose
  ----------------------- -------------------------------
  C#                      Backend programming language
  ASP.NET Core            Web API framework
  .NET 8                  Application platform
  Entity Framework Core   ORM and database access
  SQL Server              Relational database
  Swagger / OpenAPI       API documentation and testing
  LINQ                    Data querying
  REST API                Client-server communication

## Project Structure

``` text
Quiz-Competition-EF_Core/
│
├── Controllers/
│   ├── DashboardController.cs
│   ├── SubjectController.cs
│   ├── TestController.cs
│   └── WeatherForecastController.cs
│
├── DTOs/
│   └── Request/Response DTOs
│
├── Migrations/
│   └── Entity Framework Core migrations
│
├── Model/
│   ├── Subject.cs
│   ├── Test.cs
│   └── TestSubject.cs
│
├── QuizDbContext/
│   └── Entity Framework Core DbContext
│
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── QuizCompetitionEFCore.csproj
```

## Architecture

The project follows a layered Web API approach:

``` text
Client
   │
   ▼
ASP.NET Core Web API
   │
   ├── Controllers
   │      ├── Subjects
   │      ├── Tests
   │      └── Dashboard
   │
   ├── DTOs
   │
   ├── Entity Framework Core
   │
   ▼
SQL Server Database
```

## Database & Entity Framework Core

Entity Framework Core is used as the ORM layer with SQL Server as the
database provider. The project includes EF Core migrations for
maintaining and updating the database schema.

The application registers its DbContext with the SQL Server connection
configured through `appsettings.json`.

### Main Entities

-   **Subject** --- represents a quiz subject/category.
-   **Test** --- represents a quiz/test with properties such as name,
    total marks, and test date.
-   **TestSubject** --- handles the relationship between tests and
    subjects.

## API

The project exposes RESTful endpoints through ASP.NET Core controllers.

Example endpoint groups include:

``` text
GET    /api/Tests
GET    /api/Subjects
GET    /api/Dashboard
```

Additional endpoints are available for creating, updating, deleting, and
retrieving quiz-related resources.

## Swagger

Swagger/OpenAPI is enabled in the development environment, making it
easy to explore and test the available API endpoints.

After running the application, open the Swagger UI from the local
application URL:

``` text
https://localhost:<port>/swagger
```

## Getting Started

### Prerequisites

Make sure you have the following installed:

-   .NET 8 SDK
-   SQL Server or SQL Server Express/LocalDB
-   Visual Studio 2022 or VS Code
-   Entity Framework Core CLI tools

### Clone the Repository

``` bash
git clone https://github.com/Daimx1214/Quiz-Competition-EF_Core.git
cd Quiz-Competition-EF_Core
```

### Configure the Database

Update the `DefaultConnection` connection string in:

``` text
appsettings.json
```

Example:

``` json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=QuizCompetitionDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Restore Dependencies

``` bash
dotnet restore
```

### Apply Migrations

``` bash
dotnet ef database update
```

If EF Core CLI is not installed:

``` bash
dotnet tool install --global dotnet-ef
```

### Run the Application

``` bash
dotnet run
```

Then open the Swagger UI:

``` text
https://localhost:<port>/swagger
```

## API Testing

You can test the APIs using:

-   Swagger UI
-   Postman
-   `.http` request files
-   Any REST API client

## Key Learning Outcomes

This project demonstrates practical experience with:

-   ASP.NET Core Web API development
-   RESTful API design
-   Entity Framework Core
-   Code-First database development
-   SQL Server integration
-   Entity relationships
-   DTOs
-   LINQ queries
-   Asynchronous programming with `async` / `await`
-   API documentation with Swagger
-   Database migrations
-   Controller-based application architecture

## Future Improvements

Possible future enhancements include:

-   JWT authentication and role-based authorization
-   User/participant authentication
-   Quiz attempt and scoring system
-   Timer-based quiz sessions
-   Leaderboards
-   Result history
-   Admin dashboard
-   Pagination and filtering
-   Global exception handling
-   Validation with FluentValidation
-   Unit and integration testing
-   Docker support

## Author

**Daim Ali**

GitHub: [Daimx1214](https://github.com/Daimx1214)

## License

This project is intended for educational and portfolio purposes.

## Connect

[![LinkedIn](https://img.shields.io/badge/LinkedIn-Daim_Ali-blue?style=flat&logo=linkedin)](https://www.linkedin.com/in/daimx1214/)
