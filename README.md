# Program Designer API 🎯

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-success?style=for-the-badge)
![Testing](https://img.shields.io/badge/Tests-xUnit%20%7C%20Moq%20%7C%20FluentAssertions-blue?style=for-the-badge)

A robust and scalable backend system designed to model, validate, and simulate complex educational or training programs. Built using **Clean Architecture** principles and **Domain-Driven Design (DDD)** concepts.

## 🌟 Overview

The **Program Designer** allows users to define hierarchical programs consisting of **Groups** and **Steps**. Groups can contain nested groups or individual steps, and each component can define complex dependency structures (prerequisites) and ordering constraints. 

A core feature of this system is the **Validation Engine**, which mathematically guarantees the logical soundness of the designed program (e.g., detecting cycles, unreachable steps, and invalid dependencies).

## 🏗️ Architecture

This project strictly adheres to **Clean Architecture**, ensuring separation of concerns, testability, and independence from external frameworks:

- **ProgramDesigner.Domain**: Contains the core business logic, entities, value objects, and interfaces.
- **ProgramDesigner.Application**: Contains the use cases (Services), DTOs, and application logic.
- **ProgramDesigner.Infrastructure**: Implements data access (Repositories, Unit of Work) and external integrations.
- **ProgramDesigner.API**: The presentation layer providing RESTful endpoints.
- **ProgramDesigner.Tests**: Comprehensive test suites (Unit Tests) validating the core domain and application logic.

## 🛡️ Validation Engine & Test Cases

The application features a powerful validation service that checks programs for structural and logical errors before they can be simulated or executed. 

### Key Validation Test Cases Covered:

1. **Cycle Validation (`CycleValidationTests`)**
   - 🔄 Detects and prevents circular dependencies (e.g., Step A depends on Step B, which depends on Step A).
2. **Self-Reference Validation (`SelfReferenceTests`)**
   - ❌ Ensures a step cannot list itself as a prerequisite.
3. **Ordering Constraints (`OrderingValidationTests`)**
   - 📏 Validates that components within strictly ordered groups (`InOrder`) do not define prerequisites that contradict the group's chronological order.
4. **Reachability Analysis (`ReachabilityTests`)**
   - 🛤️ Ensures there are no "dead ends." Every step must be reachable and not blocked by impossible prerequisite conditions.
5. **Hierarchy and Scope Rules (`HierarchyAndReferenceTests`)**
   - 📂 Prevents components from referencing prerequisites that exist in completely isolated or parallel branches where cross-referencing is illegal.
6. **Choice Groups (`ChoiceGroupTests`)**
   - 🔀 Validates logical rules for groups where users can choose $N$ out of $M$ steps.
7. **Valid Scenarios (`ValidProgramTests`)**
   - ✅ Extensive tests ensuring standard operations (Nested Groups, Large Hierarchies, Multiple Branches) function correctly.

## 🚀 Features

- **Hierarchical Modeling**: Build complex N-level deep structures of groups and steps.
- **Advanced Dependency Management**: Define precise prerequisites across different levels of the program.
- **Simulation Engine**: Run simulations (`SimulationRequestDto` / `SimulationResultDto`) to track completion state and evaluate choice selections.
- **RESTful API**: Clean and documented endpoints for managing programs.

## 📐 Design Decisions

- **Clean Architecture & Layered Design**: Adopted to ensure a strict separation of concerns between the Domain, Application, Infrastructure, and API layers. This makes the core business logic independent of external frameworks and highly testable.
- **Repository & Unit of Work Patterns**: Used to abstract data access logic and seamlessly manage database transactions, allowing for centralized business rules and easier mocking during unit testing.
- **Entity Framework Core & SQL Server**: Chosen as the primary ORM and database engine for robust, relational data modeling and LINQ integration, facilitating efficient querying of hierarchical data.
- **DTOs (Data Transfer Objects)**: Used to decouple internal domain entities from the API contract, preventing over-posting attacks and ensuring only necessary data is exposed.
- **Global Exception Handling**: Implemented to provide consistent, structured error responses across all API endpoints, keeping the controllers clean of repetitive `try-catch` blocks.

## ⚖️ Trade-offs

- **Simplicity over Advanced Query Optimization**: Opted for straightforward EF Core queries and a standard relational schema. For extremely deep, recursive hierarchies, a graph database might be more performant, but SQL Server was chosen to reduce infrastructure complexity.
- **Stateless Simulation**: The simulation endpoint currently evaluates the program state dynamically based on the request (`SimulationRequestDto`) rather than persisting user progress. This simplifies the architecture but shifts the burden of maintaining long-running session state to the client.
- **Focus on Core Domain over Security**: Authentication and authorization (e.g., JWT, Identity) were intentionally omitted to keep the project focused strictly on the domain complexity of program validation and simulation.

## 🤖 AI Assistance Acknowledgment

To accelerate development and ensure high code quality, this project was built with the assistance of advanced AI tools:
- **ChatGPT (OpenAI)**: Assisted in analyzing and understanding the core business logic.
- **Antigravity (Google DeepMind)**: Provided extensive support with writing `xUnit` tests, developing the validation engine, and performing continuous code edits and refinements.

These tools were utilized for architectural brainstorming, test case generation, and continuous code refinement, demonstrating a modern approach to software engineering.

## ⚙️ Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code

### Running the API
1. Clone the repository.
2. Set `ProgramDesigner.API` as the startup project.
3. Run the application (F5). Swagger UI will be available to explore the endpoints.

### Running the Tests
Navigate to the root directory and run:
```bash
dotnet test
```
All unit tests utilizing `xUnit`, `Moq`, and `FluentAssertions` will execute.

---
*Developed with ❤️ and AI.*
