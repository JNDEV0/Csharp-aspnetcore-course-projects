# C# ASP.NET Core Course Projects

This repository contains a collection of projects and examples developed as part of a C# ASP.NET Core course. Each folder typically represents a specific section or topic covered in the course, demonstrating various features and concepts of the ASP.NET Core framework.

## Projects Index

Below is a list of the projects included in this repository, along with a brief description of the concepts they aim to illustrate.

*   **[ASPCourseSection30/MinimalApiExample](/ASPCourseSection30/MinimalApiExample)**
    *   **Project: Minimal API Example**
    *   **Description:** Introduces and demonstrates the use of Minimal APIs, a streamlined way to build HTTP APIs with ASP.NET Core, requiring less boilerplate code compared to traditional MVC controllers.
*   **[ASPCourseSection26](/ASPCourseSection26)**
    *   **Solution: Web API with JWT Authentication, EF Core, and Client App**
    *   **Description:** This section contains a multi-project solution demonstrating the creation of a secure Web API with versioning, data persistence using Entity Framework Core, and a client application for interaction. Key components include:
        *   **`Example.WebAPI` (API Project):**
            *   Implements versioned API controllers (e.g., `v1/CitiesController`) for managing resources.
            *   Includes an `AccountController` for user registration and login, likely integrating JWT-based authentication.
            *   Interacts with service layers (e.g., `CitiesManager` interface) for business logic.
        *   **`CitiesManager.Infra` (Infrastructure Layer):**
            *   Contains the `AppDbContext` for Entity Framework Core, defining the database schema and enabling data access.
            *   Used by EF Core migrations to create and update SQL database tables from code-first entity definitions.
        *   **`Entities` (Core Layer):**
            *   Holds domain model classes (entities mapped to database tables).
            *   Includes Data Transfer Objects (DTOs) for API request/response shaping.
            *   Contains service contracts (interfaces) for business logic abstraction.
            *   Likely includes Identity-related classes for user management and a `JwtService` for generating and validating JWT tokens.
        *   **`ClientApp` (Testing/Client Project):**
            *   A simple console application that uses `HttpClient` to send requests to the Web API.
            *   Used for testing API endpoints, including authenticated routes, and verifying responses.
*   **[ASPCourseSection15](/ASPCourseSection15)**
    *   **Solution: CRUD Operations with xUnit Testing, Service Layers, and Entity Framework Core**
    *   **Description:** This comprehensive section provides a single solution with multiple interconnected projects to demonstrate a full CRUD (Create, Read, Update, Delete) application architecture. Key concepts and projects include:
        *   **`CRUDxUnitExample` (Main Application):**
            *   Implements full CRUD operations for entities (e.g., Persons, Countries).
            *   Handles HTTP requests through MVC Controllers and routes.
            *   Performs model validation.
            *   Utilizes dependency injection to consume service classes.
            *   Interacts with a database (created via Entity Framework Core migrations) through a `DbContext`.
        *   **`CRUDTests` (Testing Project):**
            *   Focuses on unit and integration testing for the CRUD application using xUnit.
            *   Includes test cases for `PersonsService` and `CountriesService`, covering various scenarios for creating, reading, updating, and deleting data.
            *   Demonstrates writing effective test cases for service layer logic.
        *   **`Entities` (Domain Layer):**
            *   Contains the domain model classes (e.g., `Person`, `Country`) that represent the application's data structures.
        *   **`ServiceContracts` (Abstraction Layer):**
            *   Defines interfaces (e.g., `IPersonsService`, `ICountriesService`) that specify the contract for business logic operations. This promotes loose coupling and testability.
        *   **`Services` (Business Logic Layer):**
            *   Provides concrete implementations of the service contracts (e.g., `PersonsService`, `CountriesService`).
            *   Encapsulates business logic and interacts with the data access layer (DbContext) to perform CRUD operations.
            *   Designed for Inversion of Control (IoC) through dependency injection.
*   **[ASPCourseSection14/HttpClientExample](/ASPCourseSection14/HttpClientExample)**
    *   **Project: HttpClient Example**
    *   **Description:** Shows how to use `HttpClient` (or `IHttpClientFactory`) in ASP.NET Core to make HTTP requests to external APIs or services. This often includes best practices for managing `HttpClient` instances.
*   **[ASPCourseSection13/EnvironmentsExample](/ASPCourseSection13/EnvironmentsExample)**
    *   **Project: Environments Example**
    *   **Description:** Demonstrates how to configure and use different environments (e.g., Development, Staging, Production) in ASP.NET Core. This allows for environment-specific settings, middleware, and behaviors.
*   **[ASPCourseSection12](/ASPCourseSection12)**
    *   **Project: Dependency Injection Example**
    *   **Description:** In this project example, a list of cities is retrieved from a dependency-injected Inversion of Control (IoC) container. The objective is to resolve dependency issues of instantiating services directly. By making the `HomeController` and the target Service class depend on an Interface contract instead, the Routing, model, and view can be developed separately from the Service logic, which is instantiated on demand and quickly disposed of. The service called by the client's URL route request could itself have child scoped services to retrieve database code or execute other calculations before responding to the `HomeController` with the standardized data stored in local runtime memory, and then the Service instance is closed.
        *   **Goal:** To set up a way in which the Controller's action methods call on containerized service classes instead of calling on public methods instantiated in the service code files directly.
*   **[AspCourseSection11/ViewComponentsExample](/AspCourseSection11/ViewComponentsExample)**
    *   **Project: View Components Example**
    *   **Description:** Illustrates how to use View Components in ASP.NET Core. View Components are similar to partial views but are better suited for more complex reusable UI logic that might require its own controller-like actions and data retrieval.
*   **[AspCourseSection10/PartialViews](/AspCourseSection10/PartialViews)**
    *   **Project: Partial Views Example**
    *   **Description:** Demonstrates the use of Partial Views in ASP.NET Core to create reusable UI components within Razor views, helping to break down complex views into smaller, manageable pieces.
*   **[ASPCourseSection6](/ASPCourseSection6)**
    *   **This section contains multiple projects focusing on ASP.NET Core MVC fundamentals:**
        *   **[ControllerExample](./ASPCourseSection6/ControllerExample)**:
            *   **Description:** Demonstrates the basic structure and usage of Controllers and Action Methods in ASP.NET Core MVC for handling incoming web requests and returning responses.
        *   **[IActionExample (IActionResult Responses)](./ASPCourseSection6/IActionExample)**:
            *   **Description:** Showcases various `IActionResult` types that Action Methods can return (e.g., `OkObjectResult`, `JsonResult`, `ContentResult`, `FileResult`, `NotFoundResult`, `BadRequestResult`, `RedirectToActionResult`). This allows for fine-grained control over the HTTP response, including status codes, content types, and redirection.
        *   **[ModelBindingExample](./ASPCourseSection6/ModelBindingExample)**:
            *   **Description:** Illustrates the Model Binding mechanism in ASP.NET Core MVC. This project shows how incoming HTTP request data (from query strings, route parameters, form submissions, and request bodies) is automatically converted and mapped to parameters of action methods or properties of model classes.
        *   **[ModelValidationExample](./ASPCourseSection6/ModelValidationExample)**:
            *   **Description:** Covers Model Validation techniques in ASP.NET Core MVC. This includes using Data Annotations (e.g., `[Required]`, `[StringLength]`, `[Range]`) on model properties and checking `ModelState.IsValid` within action methods to validate incoming data and return appropriate feedback or error messages.
*   **[ASPCourseSection5](/ASPCourseSection5)**
    *   **Project 1: Advanced Routing, Validations, and Constraints**
        *   **Description:** This project delves into ASP.NET Core's routing capabilities using `UseRouting()` and `UseEndpoints()`. It demonstrates mapping URL patterns to specific middleware handlers, including handling different HTTP methods (e.g., `MapPost`). Key features explored are:
            *   Route variables with default values (e.g., `Files/{fileNumber}.{fileExtension=txt}`).
            *   A comprehensive set of built-in route constraints like `:int?`, `:datetime`, `:guid`, `:length(min,max)`, `:alpha`, `:range(min-max)`, `:min(value)`, and `:regex(...)`.
            *   Implementation and registration of custom route constraints (e.g., `builder.Services.AddRouting(options => { options.ConstraintMap.Add("customMonth", typeof(MonthConstraint)); })`).
            *   Middleware for inspecting the resolved endpoint using `context.GetEndpoint()`.
            *   Fallback `app.Run()` for unhandled paths.
    *   **Project 2: Serving Static Files from Custom Folders**
        *   **Description:** This project explains how to serve static files (CSS, JavaScript, images, etc.) in ASP.NET Core beyond the default `wwwroot` directory. It covers:
            *   Changing the primary static files root path using `new WebApplicationOptions() { WebRootPath = "customStaticFiles1" }` when creating the `WebApplicationBuilder`.
            *   Serving static files from additional, multiple custom folders by configuring `app.UseStaticFiles()` with a `StaticFileOptions` object that uses a `PhysicalFileProvider` pointing to a custom path (e.g., `Path.Combine(builder.Environment.ContentRootPath, "customStaticFiles2")`).
*   **[AspCourseSection4](/AspCourseSection4)**
    *   **Project: Middleware Implementation Example**
    *   **Description:** This project explores middleware implementation in ASP.NET Core, demonstrating how to create and use custom middleware components to handle requests and responses in the pipeline.
*   **[ASPCourseSection3/ASPCourseSection3Assignment](/ASPCourseSection3/ASPCourseSection3Assignment)**
    *   **Project: URI Query Based API Calculator**
    *   **Description:** This assignment demonstrates building a simple API that performs calculations based on parameters provided in the URI query string. It parses query parameters for two numbers and an operation (e.g., `?firstNumber=10&secondNumber=5&operation=add`), performs the calculation (add, subtract, multiply, divide, remainder), and returns the result as a string in the HTTP response. It also includes basic error handling for invalid operations and sets the HTTP status code accordingly.
*   **[ASPCourseSection2](/ASPCourseSection2)**
    *   **Project: HTTP Request/Response Handling Fundamentals**
    *   **Description:** This project demonstrates low-level HTTP request and response handling in ASP.NET Core using `WebApplication.Run()`. It covers inspecting request properties (path, method, query strings, headers like `User-Agent` and custom ones like `AuthenticationKey`), processing `GET` and `POST` requests (including reading the request body with `StreamReader` and parsing it), and constructing responses with custom status codes, headers (e.g., `Content-Type`, `Server`), and HTML content. Includes detailed comments on the HTTP lifecycle, Kestrel, reverse proxies, and the significance of various HTTP components.

## How to Use
1.  **Clone the repository:**
    ```bash
    git clone https://github.com/JNDEV0/Csharp-aspnetcore-course-projects.git
    cd Csharp-aspnetcore-course-projects
    ```
2.  **Navigate to a specific project folder/solution:**
    *   For most sections with a single project, navigate into the section folder (e.g., `cd ASPCourseSection2`).
    *   For sections that are solutions containing multiple projects (like `ASPCourseSection15` and `ASPCourseSection26`), navigate to the root of that section (e.g.,`cd ASPCourseSection15`). You'll typically open the `.sln` file in an IDE.
    *   For sections with multiple distinct, standalone projects (like `ASPCourseSection5` and `ASPCourseSection6`), navigate into the specific sub-project folder you wish to run (e.g., `cd ASPCourseSection6/ControllerExample`).
3.  **Open and Run the Project/Solution:**
    *   You can open the project (`.csproj`) or solution (`.sln`) file in Visual Studio or your preferred IDE.
    *   **Using .NET CLI (navigate into the specific project directory that contains the `.csproj` file, or the solution directory for `.sln`):**
        *   To run a specific project: `dotnet run --project ./PathToSpecificProject/ProjectName.csproj` (if not already in the project folder). Or simply `dotnet run` if you are in the project folder.
        *   For solutions with multiple runnable projects (like `ASPCourseSection15` and `ASPCourseSection26`), Visual Studio or JetBrains Rider is often easier to manage which project is the startup project. With the CLI, you might need to specify the startup project when running `dotnet run` from the solution directory or run each desired project separately. For example, to run the Web API in `ASPCourseSection26`, you might `cd ASPCourseSection26/Example.WebAPI` and then `dotnet run`. The `ClientApp` would be run in a separate terminal.
    *   The application will then be accessible via a local URL (e.g., `http://localhost:5000` or `https://localhost:5001`), which will be displayed in the terminal.

## Prerequisites
*   [.NET SDK](https://dotnet.microsoft.com/download) (version compatible with the projects, likely .NET 6 or .NET 7, given the topics).
*   An IDE like [Visual Studio](https://visualstudio.microsoft.com/) (recommended for solutions with multiple projects), [Visual Studio Code](https://code.visualstudio.com/) (with C# extension), or [JetBrains Rider](https://www.jetbrains.com/rider/).
*   For projects involving database interaction (like Section 15 and 26), you'll need a SQL Server instance (e.g., SQL Server Express LocalDB, which often comes with Visual Studio, or a full SQL Server instance) accessible for Entity Framework Core migrations and runtime.

---
*This README provides a general overview. For specific details about each project, including connection strings or specific setup steps for databases, please refer to the code, configuration files (e.g., `appsettings.json`), and any comments within that project's folder.*
