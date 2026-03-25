---
name: Testing Instructions
description: Standards and guidelines for writing unit and integration tests in this project.
applyTo: '**/tests/**/*.cs'
---

# Testing Instructions

Follow these conventions when generating or modifying tests in this project.

## General

- **Framework**: Use [NUnit 3](https://docs.nunit.org/) for all tests.
- **Mocking**: Use [Moq](https://github.com/devlooped/moq) for mocking dependencies in unit tests. Do not use NSubstitute.
- **Async**: All tests that call async code must be `async Task` methods.
- **Naming**: Use the pattern `MethodName_Scenario_ExpectedBehavior` (e.g., `GetAllGods_WhenRepositoryReturnsEmpty_ShouldReturnEmptyList`).

## Project Structure

- Unit tests go in `tests/UnitTests/`.
- Integration tests go in `tests/IntegrationTests/`.
- Test class names should mirror the class under test with a `Tests` suffix (e.g., `GodEndpointsTests` for `Gods`).
- One test file per class under test.

## Unit Tests

### Setup

- Use `[SetUp]` to initialize mocks and shared test state.
- Create `Mock<T>` instances for each repository interface dependency.
- Call endpoint methods directly as static methods (e.g., `Gods.AddOrUpdateGods(...)`) passing `mock.Object` as the repository argument.

### Structure

YOU MArrange-Act-Assert (AUST FOLLOW AA) with explicit comments:

```csharp
[Test]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var expected = new List<God> { new God { Name = "Zeus", MythologyId = 1, Description = "God of the sky" } };
    _mockRepository.Setup(repo => repo.GetAllGodsAsync()).ReturnsAsync(expected);

    // Act
    var result = await Gods.GetAlllGods(_mockRepository.Object);

    // Assert
    Assert.That(result.Count, Is.EqualTo(1));
}
```

### Assertions

- Use the NUnit constraint model: `Assert.That(actual, Is.EqualTo(expected))`.
- Prefer specific assertions over generic ones (e.g., `Is.EqualTo`, `Is.Not.Null`, `Is.Empty`, `Has.Count.EqualTo(n)`).
- Assert on meaningful properties, not just counts — verify names, IDs, or descriptions where relevant.

### Required Coverage per Endpoint Method

For each endpoint method, write tests covering:

1. **Happy path** — valid input returns expected output.
2. **Empty results** — repository returns an empty collection.
3. **Single item** — repository returns exactly one item.
4. **Multiple items** — repository returns several items with distinct values.
5. **Error/exception handling** — repository throws an exception (verify it propagates or is handled).

### Endpoint-Specific Guidance

- **GET all** (`GetAlllGods`): Test empty list, single god, multiple gods.
- **GET by ID** (`GetGodAsync`): Test valid ID returns god, verify correct `GodParameter` is passed.
- **GET search by name** (`GetGodByNameAsync`): Test matching name, no match, and `includeAliases` flag (both `true` and `false`).
- **POST add/update** (`AddOrUpdateGods`): Test adding new gods, updating existing gods (with `Id` set), mixed add/update in one call.
- **DELETE all** (`DeleteAllGods`): Test returns `NoContent` (204) result on success, test exception scenario.

## Integration Tests

### Setup

- Use `CustomWebApplicationFactory<Program>` to create the test server.
- Create and dispose `HttpClient` and factory in `[SetUp]` / `[TearDown]`.
- Target actual HTTP routes (e.g., `/api/v1/gods`).

### Structure

Follow the same AAA pattern with comments as unit tests.

### Required Coverage per Endpoint

1. **Status code** — verify the response returns the expected HTTP status code.
2. **Response body** — deserialize and assert on the returned data shape and content.
3. **Error responses** — test invalid requests return appropriate error codes (400, 404, 500).
4. **Concurrent/rate-limiting** — where applicable, verify rate-limiting behavior.

### HTTP Testing Patterns

```csharp
// GET
var response = await _httpClient.GetAsync("/api/v1/gods");
Assert.That(response.IsSuccessStatusCode, Is.True);

// GET with deserialization
var gods = await _httpClient.GetFromJsonAsync<List<God>>("/api/v1/gods");
Assert.That(gods, Is.Not.Null);

// POST
var response = await _httpClient.PostAsJsonAsync("/api/v1/gods", godInputs);
Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

// DELETE
var response = await _httpClient.DeleteAsync("/api/v1/gods");
Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
```

## Domain Models Reference

When constructing test data, use these models:

- `God` (from `MythApi.Common.Database.Models`): `Id`, `Name`, `Description`, `MythologyId`, `Aliases`
- `GodInput` (from `MythApi.Gods.Models`): `Id?`, `Name`, `Description`, `MythologyId`
- `Mythology` (from `MythApi.Common.Database.Models`): `Id`, `Name`, `Gods`
- `Alias` (from `MythApi.Common.Database.Models`): `Id`, `GodId`, `Name`
- `GodParameter`: record with `int Id`
- `GodByNameParameter`: record with `string Name`, `bool IncludeAliases`

## What NOT to Do

- Do not use `[TestCase]` for parameterized tests unless the scenario genuinely varies only by input/output values.
- Do not test framework behavior (e.g., routing, DI wiring) in unit tests — that belongs in integration tests.
- Do not share mutable state between tests; reset everything in `[SetUp]`.
- Do not suppress or catch exceptions in tests — let NUnit report failures naturally.
