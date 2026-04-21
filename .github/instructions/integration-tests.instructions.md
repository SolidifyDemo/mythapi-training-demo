---
name: Integration Test Instructions
description: Standards and conventions for writing integration tests in tests/IntegrationTests/.
applyTo: '**/tests/IntegrationTests/**/*.cs'
---

# Integration Test Instructions

Follow these conventions when generating or modifying integration tests in `tests/IntegrationTests/`.

## Framework & Libraries

- **Test framework**: [NUnit 3](https://docs.nunit.org/)
- **HTTP testing**: `Microsoft.AspNetCore.Mvc.Testing` — use `WebApplicationFactory<Program>`
- **No mocking library** — integration tests run against the real application stack with a seeded SQLite database.
- **Async**: All tests calling async code must be `async Task` methods.

## File & Class Conventions

- One test file per endpoint resource (e.g., `GodsEndpointTests.cs` for `/api/v1/gods`).
- Test class name mirrors the resource with an `EndpointTests` suffix (e.g., `GodsEndpointTests`).
- Namespace: `IntegrationTests`.
- Decorate test classes with `[TestFixture]`.

## Naming

Use the pattern `MethodName_Scenario_ExpectedBehavior`:

```
GetAllGods_ShouldReturnSuccessStatusCode
GetMythologyById_WithInvalidId_ShouldReturnNotFound
GetAllGods_ConcurrentRequests_ShouldRespectRateLimit
```

## Setup & Teardown

Create and dispose both `CustomWebApplicationFactory<Program>` and `HttpClient` per test class using `[SetUp]` and `[TearDown]`:

```csharp
private CustomWebApplicationFactory<Program> _factory;
private HttpClient _httpClient;

[SetUp]
public void SetUp()
{
    _factory = new CustomWebApplicationFactory<Program>();
    _httpClient = _factory.CreateClient();
}

[TearDown]
public void TearDown()
{
    _httpClient.Dispose();
    _factory.Dispose();
}
```

## Test Structure

Follow Arrange-Act-Assert (AAA) with explicit comments. For tests that require a known valid ID, fetch the resource list first in the Arrange step.

```csharp
[Test]
public async Task GetMythologyById_WithValidId_ShouldReturnCorrectMythology()
{
    // Arrange
    var mythologies = await _httpClient.GetFromJsonAsync<List<Mythology>>("/api/v1/mythologies");
    var expected = mythologies!.First();

    // Act
    var result = await _httpClient.GetFromJsonAsync<Mythology>($"/api/v1/mythologies/{expected.Id}");

    // Assert
    Assert.That(result!.Name, Is.EqualTo(expected.Name));
}
```

## HTTP Patterns

- Use `GetAsync`, `PostAsJsonAsync`, `PutAsJsonAsync`, `DeleteAsync` for raw response access.
- Use `GetFromJsonAsync<T>` / `ReadFromJsonAsync<T>` when you need to deserialize the response body.
- API base path: `/api/v1/{resource}`

```csharp
// Status code check
var response = await _httpClient.GetAsync("/api/v1/gods");
Assert.That(response.IsSuccessStatusCode, Is.True);
Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

// Deserialized body check
var gods = await _httpClient.GetFromJsonAsync<List<God>>("/api/v1/gods");
Assert.That(gods, Is.Not.Null);
```

## Assertions

- Use the NUnit constraint model: `Assert.That(actual, constraint)`.
- One assertion per test to keep failures focused and clear.
- Prefer specific matchers: `Is.EqualTo`, `Is.Not.Null`, `Is.True`, `Is.Empty`, `Has.Count.GreaterThan(0)`.
- Assert on meaningful properties (name, ID) — not just status codes alone.

## Required Coverage per Endpoint

For each endpoint, write tests covering:

1. **Status code** — verify the response returns the expected HTTP status code.
2. **Response body** — deserialize and assert on the returned data shape and content.
3. **Not found** — verify invalid/nonexistent IDs return `404 NotFound`.
4. **Seeded data** — verify responses reflect data seeded by `DatabaseInitializer`.
5. **Rate limiting** — use `Task.WhenAll` with concurrent requests to verify `429 TooManyRequests` is returned once the limit is exceeded.

## Rate Limiting Test Pattern

```csharp
[Test]
public async Task GetAllGods_ConcurrentRequests_ShouldRespectRateLimit()
{
    // Arrange
    const int numberOfRequests = 100;
    var tasks = new Task<HttpResponseMessage>[numberOfRequests];

    // Act
    for (var i = 0; i < numberOfRequests; i++)
    {
        tasks[i] = _httpClient.GetAsync("/api/v1/gods");
    }
    var responses = await Task.WhenAll(tasks);

    // Assert
    var successfulRequests = responses.Count(r => r.IsSuccessStatusCode);
    Assert.That(successfulRequests, Is.LessThanOrEqualTo(100));
}
```

## Seeded Data

Tests rely on data seeded by `DatabaseInitializer` at application startup via `CustomWebApplicationFactory`. Do not hardcode IDs — always resolve valid IDs dynamically by first fetching the resource list.

## CustomWebApplicationFactory

The factory is defined in `CustomWebApplicationFactory.cs` and configures the app to run in the `Development` environment with SQLite. Do not modify factory behavior inside individual test files — extend `CustomWebApplicationFactory` instead.
