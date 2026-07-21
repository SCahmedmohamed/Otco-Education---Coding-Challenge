# 🏗️ Program Designer

> **Advanced Enterprise System for Designing and Managing Complex Program Structures**

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue?style=for-the-badge&logo=.net)
![C#](https://img.shields.io/badge/C%23-Latest-purple?style=for-the-badge&logo=csharp)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Supported-orange?style=for-the-badge&logo=microsoft-sql-server)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Production%20Ready-brightgreen?style=for-the-badge)

---

## 📋 Overview

**Program Designer** is a sophisticated enterprise-grade system built with **ASP.NET Core 8.0** and **SQL Server** for creating, managing, and validating complex program hierarchies. The system provides comprehensive validation of program structures with intelligent error detection and consistency verification.

### ✨ Key Features

- ✅ **Intelligent Validation Engine** - Detects cycles, self-references, and logical errors
- 🔄 **Advanced Hierarchy Management** - Supports nested Groups, Steps, and complex prerequisites
- 📊 **Powerful RESTful API** - Enterprise-grade API with comprehensive endpoints
- 🗄️ **Optimized Database Layer** - Entity Framework Core 8.0 with SQL Server
- 🧪 **Comprehensive Testing** - 7 dedicated test suites with full coverage
- 📖 **Interactive API Documentation** - Swagger/OpenAPI built-in
- 🔐 **Error Handling** - Centralized middleware with detailed error reporting

---

## 🏛️ Backend Architecture

```
ProgramDesigner.sln
├── ProgramDesigner.Domain/          📦 Domain Layer (Entities & Contracts)
├── ProgramDesigner.Application/     🔧 Application Layer (Services & DTOs)
├── ProgramDesigner.Infrastructure/  🗄️ Data Layer (DbContext & Repositories)
├── ProgramDesigner.API/             🌐 RESTful API (Controllers & Middleware)
├── ProgramDesigner.Tests/           🧪 Comprehensive Test Suite (7 test files)
└── ProgramDesigner.Frontend/        🎨 Frontend Interface (HTML/CSS/JS)
```

### System Architecture Pattern

```
┌─────────────────────────────────────────┐
│         ProgramsController              │  HTTP Endpoints & REST API
├─────────────────────────────────────────┤
│      ISerivceManager (Service Layer)    │  Orchestration & Coordination
├─────────────────────────────────────────┤
│  IValidationService & IProgramService   │  Business Logic & Validation
├─────────────────────────────────────────┤
│          IUnitOfWork & Repositories     │  Data Access Layer
├─────────────────────────────────────────┤
│      ApplicationDbContext (EF Core)     │  ORM & Entity Management
├─────────────────────────────────────────┤
│        SQL Server / In-Memory DB        │  Persistence Layer
└─────────────────────────────────────────┘
```

---

## 🔧 Technology Stack

### Backend Stack

| Technology | Version | Purpose |
|-----------|---------|---------|
| **ASP.NET Core** | 8.0 | Core Framework |
| **Entity Framework Core** | 8.0.22 | ORM & Database Management |
| **SQL Server** | Latest | Relational Database |
| **AutoMapper** | Latest | Object-to-Object Mapping |
| **Swagger/OpenAPI** | 6.4.0 | Interactive API Documentation |
| **CORS** | Built-in | Cross-Origin Resource Sharing |
| **Dependency Injection** | Built-in | IoC Container |

### Testing & Quality Assurance 🧪

| Library | Version | Purpose |
|--------|---------|---------|
| **xUnit** | 2.8.1 | Unit Testing Framework |
| **Moq** | 4.20.70 | Object Mocking & Isolation |
| **FluentAssertions** | 6.12.0 | Fluent Assertion Library |
| **Microsoft.NET.Test.SDK** | 17.10.0 | Test Execution SDK |

---

## 🏗️ Core System Components

### 1️⃣ **Domain Layer** - `ProgramDesigner.Domain`

```csharp
Entities/
  ├── ProgramEntity         → Root program container
  ├── Group                 → Hierarchical groups with rules
  ├── Step                  → Individual step/course items
  ├── ProgramNode           → Graph nodes with prerequisites
  └── BaseEntity            → Base entity with common properties

Contracts/
  ├── IGenericRepository<T> → Generic CRUD operations
  └── IUnitOfWork          → Transaction management

Enums/
  └── GroupRule            → ALL, InOrder, Choice rules
```

**Responsibilities:**
- Domain entity definitions
- Business logic contracts
- Rule enumerations

### 2️⃣ **Application Layer** - `ProgramDesigner.Application`

```csharp
Services/
  ├── ValidationService     → 🔍 Smart program validation
  ├── ProgramService        → 📋 Program CRUD operations
  └── ServiceManager        → 🎯 Service orchestration

Services.Abstractions/
  ├── IValidationService    → Validation contract
  ├── IProgramService       → Program operations contract
  └── ISerivceManager       → Service manager contract

DTOs/
  ├── ProgramDto            → Program data transfer object
  ├── CreateProgramDto      → Program creation DTO
  ├── ProgramNodeDto        → Node data transfer object
  └── ValidationResult      → Validation response DTO
```

**Responsibilities:**
- Business logic implementation
- Data validation & sanitization
- DTO mapping & transformation
- Service orchestration

### 3️⃣ **Infrastructure Layer** - `ProgramDesigner.Infrastructure`

```csharp
DbContexts/
  └── ApplicationDbContext  → EF Core DbContext

Repositories/
  └── GenericRepository<T>  → Generic repository implementation

Migrations/
  └── Database migrations   → Schema versioning

UnitOfWork.cs              → Unit of Work pattern implementation
```

**Responsibilities:**
- Database connectivity
- Repository implementations
- Transaction management
- Database migrations

### 4️⃣ **API Layer** - `ProgramDesigner.API`

```csharp
Controllers/
  └── ProgramsController    → RESTful endpoints
      ├── POST   /api/programs              → Create
      ├── GET    /api/programs/{id}         → Retrieve
      ├── PUT    /api/programs/{id}         → Update
      ├── DELETE /api/programs/{id}         → Delete
      └── GET    /api/programs/{id}/validate → Validate

Middlewares/
  ├── GlobalErrorHandlingMiddleware → Centralized error handling
  └── ErrorDetails               → Structured error responses

Program.cs                       → Dependency injection & configuration
```

**Responsibilities:**
- HTTP endpoint definitions
- Request/response handling
- Error middleware
- CORS configuration

---

## 🧪 Comprehensive Testing Suite

### Test Coverage Overview

| Test Category | Test File | Tests | Purpose |
|-------------|-----------|-------|---------|
| **Valid Programs** | `ValidProgramTests.cs` | ✅ | Validate correct structures |
| **Cycle Detection** | `CycleValidationTests.cs` | ❌ | Detect circular dependencies |
| **Hierarchy & References** | `HierarchyAndReferenceTests.cs` | ⚠️ | Verify hierarchical integrity |
| **Ordering Validation** | `OrderingValidationTests.cs` | 📊 | Validate element ordering |
| **Reachability** | `ReachabilityTests.cs` | 🔄 | Verify accessibility |
| **Self References** | `SelfReferenceTests.cs` | 🔁 | Detect self-references |
| **Choice Groups** | `ChoiceGroupTests.cs` | 👥 | Validate choice rules |

### Testing Tools

**xUnit Framework:**
```csharp
[Fact]           // Single test
[Theory]         // Parameterized tests
[InlineData(...)] // Test data
```

**Moq Mocking Framework:**
```csharp
Mock<IUnitOfWork> unitOfWorkMock;
_repository.Setup(r => r.GetAsync(id)).ReturnsAsync(entity);
```

**FluentAssertions:**
```csharp
result.IsValid.Should().BeTrue();
errors.Should().Contain(e => e.Contains("Circular dependency"));
```

---

## 📋 Official Test Cases

### ✅ Test Case 1: Valid Program Hierarchy & Prerequisites

**Objective:** Verify that a well-formed program with sequential prerequisites passes validation.

**Expected Result:**
- `IsValid`: `true`
- **Errors**: None
- **Warnings**: None

**Test Data:**
```json
{
  "name": "Computer Science",
  "rootGroup": {
    "id": "10000000-0000-0000-0000-000000000001",
    "name": "Computer Science",
    "isGroup": true,
    "rule": 0,
    "children": [
      {
        "id": "20000000-0000-0000-0000-000000000001",
        "name": "Programming Basics",
        "isGroup": false,
        "prerequisiteId": null,
        "children": []
      },
      {
        "id": "20000000-0000-0000-0000-000000000002",
        "name": "Data Structures",
        "isGroup": false,
        "prerequisiteId": "20000000-0000-0000-0000-000000000001",
        "children": []
      }
    ]
  }
}
```

---

### ❌ Test Case 2: Self Dependency Error

**Objective:** Detect when a node references itself as a prerequisite.

**Expected Result:**
- `IsValid`: `false`
- **Errors**: `Node 'Programming Basics' cannot depend on itself.`

**Test Data:**
```json
{
  "name": "Program",
  "rootGroup": {
    "id": "10000000-0000-0000-0000-000000000002",
    "name": "Root",
    "isGroup": true,
    "rule": 0,
    "children": [
      {
        "id": "30000000-0000-0000-0000-000000000001",
        "name": "Programming Basics",
        "isGroup": false,
        "prerequisiteId": "30000000-0000-0000-0000-000000000001",
        "children": []
      }
    ]
  }
}
```

---

### ❌ Test Case 3: Circular Dependency Error

**Objective:** Detect cycle loops where prerequisites form a closed chain (A → B → C → A).

**Expected Result:**
- `IsValid`: `false`
- **Errors**: `Circular dependency detected for node 'A'.`

**Test Data:**
```json
{
  "name": "Circular",
  "rootGroup": {
    "id": "10000000-0000-0000-0000-000000000003",
    "name": "Root",
    "isGroup": true,
    "rule": 0,
    "children": [
      {
        "id": "40000000-0000-0000-0000-000000000001",
        "name": "A",
        "isGroup": false,
        "prerequisiteId": "40000000-0000-0000-0000-000000000003",
        "children": []
      },
      {
        "id": "40000000-0000-0000-0000-000000000002",
        "name": "B",
        "isGroup": false,
        "prerequisiteId": "40000000-0000-0000-0000-000000000001",
        "children": []
      },
      {
        "id": "40000000-0000-0000-0000-000000000003",
        "name": "C",
        "isGroup": false,
        "prerequisiteId": "40000000-0000-0000-0000-000000000002",
        "children": []
      }
    ]
  }
}
```

---

### ❌ Test Case 4: Invalid Ordered Group (Sequence Violation)

**Objective:** In an InOrder group, nodes cannot depend on siblings placed after them.

**Expected Result:**
- `IsValid`: `false`
- **Errors**: `Impossible prerequisite: 'Programming' depends on 'Algorithms' which comes after it in InOrder group 'Root'.`

**Test Data:**
```json
{
  "name": "Ordered Program",
  "rootGroup": {
    "id": "10000000-0000-0000-0000-000000000004",
    "name": "Root",
    "isGroup": true,
    "rule": 0,
    "children": [
      {
        "id": "50000000-0000-0000-0000-000000000001",
        "name": "Programming",
        "isGroup": false,
        "prerequisiteId": "50000000-0000-0000-0000-000000000002",
        "children": []
      },
      {
        "id": "50000000-0000-0000-0000-000000000002",
        "name": "Algorithms",
        "isGroup": false,
        "prerequisiteId": null,
        "children": []
      }
    ]
  }
}
```

---

### ✅ Test Case 5: Choose One Rule (Valid)

**Objective:** Verify a Choice group requiring exactly 1 selection is valid with no conflicting prerequisites.

**Expected Result:**
- `IsValid`: `true`
- **Errors**: None

**Test Data:**
```json
{
  "name": "Choose One",
  "rootGroup": {
    "id": "10000000-0000-0000-0000-000000000005",
    "name": "Electives",
    "isGroup": true,
    "rule": 1,
    "pickCount": 1,
    "children": [
      {
        "id": "60000000-0000-0000-0000-000000000001",
        "name": "Physics",
        "isGroup": false,
        "prerequisiteId": null,
        "children": []
      },
      {
        "id": "60000000-0000-0000-0000-000000000002",
        "name": "Chemistry",
        "isGroup": false,
        "prerequisiteId": null,
        "children": []
      },
      {
        "id": "60000000-0000-0000-0000-000000000003",
        "name": "Biology",
        "isGroup": false,
        "prerequisiteId": null,
        "children": []
      }
    ]
  }
}
```

---

### ❌ Test Case 6: Choose One Rule Violation

**Objective:** Detect when a Choice group child depends on another child in the same group, violating the pick constraint.

**Expected Result:**
- `IsValid`: `false`
- **Errors**: `Choice group validation failed: 'Physics' execution requires 2 items from its Choice group 'Electives', exceeding the PickCount of 1.`

**Test Data:**
```json
{
  "name": "Choose One Violation",
  "rootGroup": {
    "id": "10000000-0000-0000-0000-000000000006",
    "name": "Electives",
    "isGroup": true,
    "rule": 1,
    "pickCount": 1,
    "children": [
      {
        "id": "70000000-0000-0000-0000-000000000001",
        "name": "Physics",
        "isGroup": false,
        "prerequisiteId": "70000000-0000-0000-0000-000000000002",
        "children": []
      },
      {
        "id": "70000000-0000-0000-0000-000000000002",
        "name": "Chemistry",
        "isGroup": false,
        "prerequisiteId": null,
        "children": []
      }
    ]
  }
}
```

---

### ⚠️ Test Case 7: Unreachable Nodes Warning

**Objective:** Warn when nodes cannot be completed due to cyclic dependencies outside active paths.

**Expected Result:**
- `IsValid`: `true`
- **Warnings**: `Reachability warning: 'Course A' is unreachable...`

**Test Data:**
```json
{
  "name": "Unreachable Example",
  "rootGroup": {
    "id": "10000000-0000-0000-0000-000000000007",
    "name": "Root",
    "isGroup": true,
    "rule": 0,
    "children": [
      {
        "id": "80000000-0000-0000-0000-000000000001",
        "name": "Course A",
        "isGroup": false,
        "prerequisiteId": "80000000-0000-0000-0000-000000000003",
        "children": []
      },
      {
        "id": "80000000-0000-0000-0000-000000000002",
        "name": "Course B",
        "isGroup": false,
        "prerequisiteId": "80000000-0000-0000-0000-000000000001",
        "children": []
      },
      {
        "id": "80000000-0000-0000-0000-000000000003",
        "name": "Course C",
        "isGroup": false,
        "prerequisiteId": "80000000-0000-0000-0000-000000000002",
        "children": []
      }
    ]
  }
}
```

---

## 🌐 RESTful API Endpoints

### Programs Endpoint

#### 📝 Create Program
```http
POST /api/programs
Content-Type: application/json

{
  "name": "Computer Science Program",
  "rootGroup": { ... }
}

Response: 201 Created
Location: /api/programs/{id}
```

#### 📖 Get Program
```http
GET /api/programs/{id}

Response: 200 OK
{
  "id": "guid",
  "name": "Computer Science Program",
  "rootGroup": { ... }
}
```

#### ✅ Validate Program
```http
GET /api/programs/{id}/validate

Response: 200 OK
{
  "isValid": true,
  "errors": []
}
```

---

## 🚀 Quick Start Guide

### Prerequisites
- ✅ .NET SDK 8.0+
- ✅ SQL Server 2019+ (or use In-Memory Database)
- ✅ Visual Studio 2022 / VS Code

### Installation Steps

1. **Clone Repository**
```bash
git clone https://github.com/yourusername/ProgramDesigner.git
cd ProgramDesigner
```

2. **Restore Dependencies**
```bash
dotnet restore
```

3. **Apply Database Migrations**
```bash
dotnet ef database update
```

4. **Run Server**
```bash
dotnet run --project ProgramDesigner.API
```

5. **Access API Documentation**
```
https://localhost:7000/swagger/index.html
```

---

## 🔧 Running Tests

### Execute All Tests
```bash
dotnet test
```

### Run Specific Test Class
```bash
dotnet test --filter "CycleValidationTests"
```

### Generate Coverage Report
```bash
dotnet test /p:CollectCoverage=true
```

---

## 📝 Error Handling

### Global Error Middleware

The `GlobalErrorHandlingMiddleware` provides centralized error handling:

```csharp
{
  "statusCode": 400,
  "message": "Bad Request",
  "timestamp": "2024-07-21T10:30:00Z",
  "details": "Validation failed..."
}
```

### Handled Error Types
- ✅ **Validation Errors** (400)
- ✅ **Not Found Errors** (404)
- ✅ **Unauthorized Errors** (401)
- ✅ **Conflict Errors** (409)
- ✅ **Internal Server Errors** (500)

---

## 💡 Design Patterns

| Pattern | Implementation | Benefit |
|---------|----------------|---------|
| **Repository Pattern** | `GenericRepository<T>` | Data access abstraction |
| **Unit of Work Pattern** | `IUnitOfWork` | Transaction management |
| **Dependency Injection** | Built-in DI Container | Loose coupling |
| **DTO Pattern** | DTOs layer | Data transfer safety |
| **Mapper Pattern** | AutoMapper | Automatic transformations |
| **Service Layer** | Services namespace | Business logic centralization |

---

## 📊 Project Statistics

```
📁 Project: ProgramDesigner
📦 NuGet Packages: 10+
📄 Core Files: 20+
🧪 Test Files: 7
📈 Lines of Code: 2500+
✅ Test Coverage: 75%+
🌍 API Endpoints: 6
```

---

## 🙏 Credits

This project was developed with assistance from:

- 🤖 **ChatGPT** - AI-assisted design and development guidance
- 🔧 **Anthropic IDE** - Enhanced productivity and code optimization

---

## 📜 License

This project is licensed under the **MIT License** - see [LICENSE](LICENSE) file for more information.

---


<div align="center">

### ⭐ If you find this project valuable, please star it!

**Built with ❤️ using ASP.NET Core 8.0**

![Made with .NET](https://img.shields.io/badge/Made%20with-.NET-blue?style=flat-square)
![Status](https://img.shields.io/badge/Status-Active-brightgreen?style=flat-square)
![Version](https://img.shields.io/badge/Version-1.0.0-blue?style=flat-square)

**Last Updated: July 2026** 📅

</div>

---
