# Program Designer API: Validation Test Suite

This document defines the official test suite for validating Program structures, containing both hierarchical groups and prerequisite constraints. Use these test cases to verify the API's validation logic against the specified business rules.

---

## 📋 Test Case Catalog

### 1. Valid Program Hierarchy & Prerequisites ✅
* **Objective**: Verifies that a standard, well-formed program containing sequential prerequisites passes validation.
* **Expected Result**:
  * `IsValid`: `true`
  * **Errors**: None
  * **Warnings**: None

```json
{
  "name": "Computer Science",
  "rootGroup": {
    "id": "10000000-0000-0000-0000-000000000001",
    "name": "Computer Science",
    "isGroup": true,
    "rule": 0,
    "pickCount": null,
    "prerequisiteId": null,
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

### 2. Self Dependency Error ❌
* **Objective**: Detects logical errors where a course or group references itself as its own prerequisite.
* **Expected Result**:
  * `IsValid`: `false`
  * **Errors**:
    * `Node 'Programming Basics' cannot depend on itself.`

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

### 3. Circular Dependency Error ❌
* **Objective**: Detects cycle loops in the dependency graph where prerequisites form a closed loop (e.g., A ➔ B ➔ C ➔ A).
* **Expected Result**:
  * `IsValid`: `false`
  * **Errors**:
    * `Circular dependency detected for node 'A'.` (or any participating node)

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

### 4. Invalid Ordered Group (Sequence Violation) ❌
* **Objective**: In an ordered group (Rule: `InOrder`), siblings must be completed in order. This test ensures a node cannot depend on a sibling that is placed after it.
* **Expected Result**:
  * `IsValid`: `false`
  * **Errors**:
    * `Impossible prerequisite: 'Programming' depends on 'Algorithms' which comes after it in InOrder group 'Root'.`

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

### 5. Choose One Rule (Valid) ✅
* **Objective**: Verifies that a `Choice` rule group requiring exactly 1 node selection is valid when there are no conflicting prerequisite overrides forcing multiple choices.
* **Expected Result**:
  * `IsValid`: `true`
  * **Errors**: None
  * **Warnings**: None

```json
{
  "name": "Choose One",
  "rootGroup": {
    "id": "10000000-0000-0000-0000-000000000005",
    "name": "Electives",
    "isGroup": true,
    "rule": 1,
    "pickCount": 1,
    "prerequisiteId": null,
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

### 6. Choose One Rule Violation ❌
* **Objective**: Detects invalid structures in a `Choice` group where a child node has a prerequisite on another child in the *same* `Choice` group. Since selecting the first node forces the prerequisite to be active, both nodes would be required, violating the "Pick 1" constraint.
* **Expected Result**:
  * `IsValid`: `false`
  * **Errors**:
    * `Choice group validation failed: 'Physics' execution requires 2 items from its Choice group 'Electives', exceeding the PickCount of 1.`

```json
{
  "name": "Choose One",
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

### 7. Unreachable Nodes Warning ⚠
* **Objective**: Warns the user when a subset of nodes cannot be completed due to unreachable dependency cycles outside the active hierarchy path.
* **Expected Result**:
  * `IsValid`: `true`
  * **Warnings**:
    * `Reachability warning: 'Course A' is unreachable because it requires 3 nodes from Choice group 'Root' (PickCount: ...).`

```json
{
  "name": "Unreachable Example",
  "rootGroup": {
    "id": "10000000-0000-0000-0000-000000000007",
    "name": "Root",
    "isGroup": true,
    "rule": 0,
    "pickCount": null,
    "prerequisiteId": null,
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
