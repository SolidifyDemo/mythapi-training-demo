---
name: TDD Instructions
description: This file provides instructions for Test-Driven Development (TDD) practices in the project
applyTo: '**/*.cs'
---

## 🧪 Test-Driven Development (TDD)

Always follow the TDD workflow:
- Write a failing test for the new feature or bug fix before writing any implementation code.
- Run the test to confirm it fails (red).
- Implement the minimum code required to make the test pass.
- Run the test again to confirm it passes (green).
- Refactor the code as needed, ensuring all tests remain green.
- Repeat this cycle for each new feature or change.
- All new code must be covered by tests written first.
- Use NUnit and Moq for all unit tests and mocking.
- Use the arrange-act-assert pattern and only one assertion per test.
- Ensure tests are isolated and do not depend on external state.
- MUST always use this TDD process: tests first, then implementation, then refactor.

## 🧪 Test Instructions

- Use only Moq (`Mock<T>`) for mocking dependencies. Do not use NSubstitute.
- Use **NUnit** for unit tests.
- Write tests for:
  - Controllers
  - Services
  - Critical validation and logic
- Favor **integration tests** for full API pipelines (e.g., using `WebApplicationFactory`).
- Use the arrange act assert pattern
- Only do one assertion per test
- Use the constraint model for assertions
- Ensure tests are isolated and do not depend on external state