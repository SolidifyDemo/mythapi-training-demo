---
name: Unit Test Instructions
description: Standards and conventions for writing unit tests in tests/UnitTests/.
applyTo: '**/tests/UnitTests/**/*.cs'
---

# Unit Test Instructions

Follow these conventions when generating or modifying unit tests in `tests/UnitTests/`.

## Framework & Libraries

- **Test framework**: [NUnit 3](https://docs.nunit.org/)
- **Mocking**: [Moq](https://github.com/devlooped/moq) — use `Mock<T>` for all dependencies. Do **not** use NSubstitute.
- **Async**: All tests calling async code must be `async Task` methods.

## File & Class Conventions

- One test file per class under test.
- Test class name mirrors the class under test with a `Tests` suffix (e.g., `GodEndpointsTests` for `Gods`).
- Place test files in `tests/UnitTests/`.
- Namespace: `UnitTests`.

## Naming

Use the pattern `MethodName_Scenario_ExpectedBehavior`:

```
GetAllGods_WhenRepositoryReturnsEmpty_ShouldReturnEmptyList
AddOrUpdateGods_WhenNewGodProvided_ShouldReturnCreatedGod
DeleteAllGods_WhenCalled_ShouldReturnNoContent
```

## Setup

- Declare `Mock<T>` fields for each repository dependency at class level.
- Initialize all mocks in a `[SetUp]` method.

```csharp
private Mock<IGodRepository> _mockRepository;

[SetUp]
public void Setup()
{
    _mockRepository = new Mock<IGodRepository>();
}
```

## Test Structure

Follow Arrange-Act-Assert (AAA) with explicit comments. Call endpoint methods **directly as static methods**, passing `mock.Object` as the repository argument.

```csharp
[Test]
public async Task GetAllGods_WhenRepositoryReturnsTwo_ShouldReturnBothGods()
{
    // Arrange
    var expected = new List<God>
    {
        new God { Name = "Zeus", MythologyId = 1, Description = "God of the sky" },
        new God { Name = "Hera", MythologyId = 1, Description = "Goddess of marriage" }
    };
    _mockRepository.Setup(repo => repo.GetAllGodsAsync()).ReturnsAsync(expected);

    // Act
    var result = await Gods.GetAlllGods(_mockRepository.Object);

    // Assert
    Assert.That(result.Count, Is.EqualTo(2));
}
```

## Assertions

- Use the NUnit constraint model: `Assert.That(actual, constraint)`.
- One assertion per test to keep failures focused and clear.
- Prefer specific matchers: `Is.EqualTo`, `Is.Null`, `Is.Not.Null`, `Is.Empty`, `Has.Count.EqualTo(n)`.
- Assert on meaningful properties (name, ID, description) — not just counts.

```csharp
Assert.That(result.First().Name, Is.EqualTo("Zeus"));
Assert.That(result, Is.Empty);
Assert.That(result, Is.Not.Null);
```

## Mock Setup Patterns

```csharp
// Return a value
_mockRepository.Setup(repo => repo.GetAllGodsAsync()).ReturnsAsync(gods);

// Match any argument
_mockRepository.Setup(repo => repo.AddOrUpdateGods(It.IsAny<List<GodInput>>())).ReturnsAsync(gods);

// Match specific argument
_mockRepository.Setup(repo => repo.GetGodAsync(new GodParameter(1))).ReturnsAsync(god);

// Throw an exception
_mockRepository.Setup(repo => repo.GetAllGodsAsync()).ThrowsAsync(new Exception("DB error"));
```

## Required Coverage per Endpoint Method

For every endpoint method, write tests covering:

| Scenario | Description |
|---|---|
| Happy path | Valid input returns expected output |
| Empty result | Repository returns an empty collection |
| Single item | Repository returns exactly one item |
| Multiple items | Repository returns several items with distinct values |
| Exception | Repository throws — verify it propagates or is handled correctly |

### Endpoint-Specific Guidance

- **`GetAlllGods`**: Test empty list, single god, multiple gods with distinct names.
- **`GetGodAsync`**: Test valid ID returns correct god; verify the correct `GodParameter` is passed to the repository.
- **`GetGodByNameAsync`**: Test matching name, no match, `includeAliases = true`, and `includeAliases = false`.
- **`AddOrUpdateGods`**: Test adding new gods (no Id), updating existing gods (Id set), and a mixed add/update batch.
- **`DeleteAllGods`**: Test returns `IResult` with 204 No Content on success; test exception scenario.
