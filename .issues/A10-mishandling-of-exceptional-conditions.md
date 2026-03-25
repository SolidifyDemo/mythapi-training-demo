# OWASP A10:2025 - Mishandling of Exceptional Conditions

**Severity:** High  
**OWASP Category:** A10:2025 - Mishandling of Exceptional Conditions  
**Status:** Open  
**Date Identified:** March 20, 2026

## Affected Files

- [src/Program.cs](../src/Program.cs) - Missing global exception handling middleware
- [src/Gods/DBRepositories/GodRepository.cs](../src/Gods/DBRepositories/GodRepository.cs#L57) - Uses `FirstAsync()` that throws unhandled exceptions
- [src/Gods/DBRepositories/GodRepository.cs](../src/Gods/DBRepositories/GodRepository.cs#L61) - SQL injection vulnerability in search query
- [src/Mythologies/DBRepository/MythologyRepository.cs](../src/Mythologies/DBRepository/MythologyRepository.cs#L25) - Returns null without proper handling
- [src/Endpoints/v1/Gods.cs](../src/Endpoints/v1/Gods.cs#L32) - Endpoint lacks exception handling and validation

---

## 1. Description

The MythAPI application mishandles exceptional conditions in multiple critical areas, violating OWASP A10:2025 guidelines. The application lacks a robust global exception handling strategy and contains code patterns that throw unhandled exceptions or fail to properly handle error conditions. This creates a poor user experience, exposes sensitive implementation details, and can lead to application instability.

The primary issues include:
- **No global exception handler** configured in the middleware pipeline
- **Unsafe database query patterns** that throw `InvalidOperationException` when records are not found
- **Missing validation** for input parameters and edge cases
- **Lack of standardized error responses** (no ProblemDetails implementation)
- **SQL injection vulnerability** in the god search functionality

---

## 2. Details

### 2.1 Missing Global Exception Handling

**Location:** [src/Program.cs](../src/Program.cs)

The application does not configure any global exception handling middleware. The ASP.NET Core pipeline lacks:
- `UseExceptionHandler()` middleware for catching unhandled exceptions
- `UseStatusCodePages()` for handling HTTP error status codes
- ProblemDetails configuration for RFC 7807 compliant error responses

**Current Code:**
```csharp
app.RegisterGodEndpoints();
app.RegisterMythologiesEndpoints();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
```

**Impact:** Unhandled exceptions propagate to the client with default error pages, potentially exposing:
- Stack traces in development environments
- Database schema information
- Internal server paths and configuration details
- .NET framework version information

### 2.2 Throwing Exceptions on Missing Data

**Location:** [src/Gods/DBRepositories/GodRepository.cs](../src/Gods/DBRepositories/GodRepository.cs#L57)

The `GetGodAsync` method uses `FirstAsync()` which throws an `InvalidOperationException` when a god with the specified ID does not exist:

```csharp
public async Task<God> GetGodAsync(GodParameter parameter)
{
    return await _context.Gods.FirstAsync(x => x.Id == parameter.Id);
}
```

**Problem:** This violates the principle that "not found" is not an exceptional condition in a REST API—it's a valid business scenario that should return HTTP 404, not throw an exception.

**Exception Thrown:**
```
System.InvalidOperationException: Sequence contains no elements
```

### 2.3 Unhandled Null Returns

**Location:** [src/Mythologies/DBRepository/MythologyRepository.cs](../src/Mythologies/DBRepository/MythologyRepository.cs#L25)

While this repository correctly uses `FirstOrDefaultAsync()`, the nullable return type is not properly handled in the endpoint layer:

```csharp
public async Task<Mythology?> GetMythologyByIdAsync(int id)
{
    return await _context.Mythologies.FirstOrDefaultAsync(m => m.Id == id);
}
```

The endpoint that consumes this method does not check for null before serializing the response, which could lead to unexpected behavior or null reference exceptions.

### 2.4 SQL Injection Vulnerability

**Location:** [src/Gods/DBRepositories/GodRepository.cs](../src/Gods/DBRepositories/GodRepository.cs#L61-L64)

The god search functionality constructs raw SQL queries using string interpolation, creating a critical SQL injection vulnerability:

```csharp
public Task<List<God>> GetGodByNameAsync(GodByNameParameter parameter)
{
    var query = parameter.IncludeAliases 
        ? $"SELECT * FROM God WHERE Name LIKE '%{parameter.Name}%' or Id in (SELECT GodId FROM Alias WHERE Name LIKE '%{parameter.Name}%')" 
        : $"SELECT * FROM God WHERE Name LIKE '%{parameter.Name}%'";
    var result = _context.Gods.FromSqlRaw(query).ToList();
    return Task.FromResult(result);
}
```

**Attack Example:**
```
GET /api/v1/gods/search/'; DROP TABLE God; --
```

This vulnerability allows attackers to:
- Execute arbitrary SQL commands
- Exfiltrate sensitive data
- Modify or delete database records
- Potentially compromise the database server

### 2.5 Missing Input Validation

**Location:** [src/Endpoints/v1/Gods.cs](../src/Endpoints/v1/Gods.cs)

The endpoints lack validation for:
- **Negative or zero IDs** - Endpoints accept any integer without range validation
- **Invalid mythology IDs** - Foreign key violations are not caught proactively
- **Null or empty strings** - Name searches don't validate input
- **Oversized payloads** - No limits on batch operations

**Example:**
```csharp
gods.MapGet("{id}", (int id, IGodRepository repository) 
    => repository.GetGodAsync(new GodParameter(id)));
```

This accepts any integer, including negative values like `-1` or `0`, which are semantically invalid for database IDs.

---

## 3. Implications

### 3.1 Security Implications

1. **Information Disclosure (CWE-209)**
   - Unhandled exceptions expose stack traces and internal implementation details
   - Database schema and entity names are revealed through Entity Framework exceptions
   - File paths and configuration values may be leaked in error messages

2. **SQL Injection (CWE-89)**
   - Critical vulnerability allowing arbitrary SQL execution
   - Potential for data exfiltration, modification, or deletion
   - Risk of database server compromise

3. **Denial of Service**
   - Repeated exceptions can degrade performance or crash the application
   - SQL injection attacks can be used to lock tables or consume resources
   - Missing rate limiting on endpoints increases attack surface

### 3.2 Reliability Implications

1. **Application Instability**
   - Unhandled exceptions can terminate request processing
   - Cascading failures when database operations fail
   - No graceful degradation when dependencies are unavailable

2. **Poor User Experience**
   - Generic 500 errors instead of meaningful HTTP status codes
   - Inconsistent error response formats
   - Missing correlation IDs for troubleshooting

3. **Operational Challenges**
   - Difficult to diagnose issues without standardized error logging
   - No structured error responses for client applications
   - Missing observability for exception patterns and frequencies

### 3.3 Compliance Implications

- **OWASP Top 10 2025:** Direct violation of A10 - Mishandling of Exceptional Conditions
- **PCI DSS 6.5.5:** Improper error handling
- **GDPR Article 32:** Inadequate security measures to protect data
- **ISO 27001:** Insufficient logging and monitoring controls

---

## 4. Implementation of Fix

### 4.1 Configure Global Exception Handling

**File:** [src/Program.cs](../src/Program.cs)

Add global exception handling middleware with ProblemDetails support:

```csharp
var app = builder.Build();

// Configure global exception handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
            var exception = exceptionHandlerFeature?.Error;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred while processing your request",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Detail = app.Environment.IsDevelopment() 
                    ? exception?.Message 
                    : "Please contact support if the problem persists",
                Instance = context.Request.Path
            };

            // Add correlation ID for tracking
            if (context.TraceIdentifier != null)
            {
                problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            }

            // Log the exception
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(exception, "Unhandled exception occurred: {Message}", exception?.Message);

            await context.Response.WriteAsJsonAsync(problemDetails);
        });
    });
}
else
{
    // In development, show detailed error pages
    app.UseDeveloperExceptionPage();
}

// Add status code pages for proper HTTP error handling
app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 404)
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Detail = "The requested resource was not found",
            Instance = context.HttpContext.Request.Path
        };
        await context.HttpContext.Response.WriteAsJsonAsync(problemDetails);
    }
});

app.RegisterGodEndpoints();
app.RegisterMythologiesEndpoints();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
```

### 4.2 Fix Repository Query Patterns

**File:** [src/Gods/DBRepositories/GodRepository.cs](../src/Gods/DBRepositories/GodRepository.cs)

Replace `FirstAsync()` with `FirstOrDefaultAsync()` and handle null:

```csharp
public async Task<God?> GetGodAsync(GodParameter parameter)
{
    return await _context.Gods
        .Include(g => g.Aliases)
        .FirstOrDefaultAsync(x => x.Id == parameter.Id);
}
```

### 4.3 Update Endpoint to Handle Null

**File:** [src/Endpoints/v1/Gods.cs](../src/Endpoints/v1/Gods.cs)

Update the endpoint to return proper HTTP 404 when god is not found:

```csharp
gods.MapGet("{id}", async (int id, IGodRepository repository) => 
{
    if (id <= 0)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid ID",
            detail: "God ID must be a positive integer",
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        );
    }

    var god = await repository.GetGodAsync(new GodParameter(id));
    
    if (god == null)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "God not found",
            detail: $"No god exists with ID {id}",
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.4"
        );
    }

    return Results.Ok(god);
})
.Produces<God>(StatusCodes.Status200OK)
.Produces<ProblemDetails>(StatusCodes.Status404NotFound)
.Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
```

### 4.4 Fix SQL Injection Vulnerability

**File:** [src/Gods/DBRepositories/GodRepository.cs](../src/Gods/DBRepositories/GodRepository.cs)

Replace raw SQL with parameterized LINQ queries:

```csharp
public async Task<List<God>> GetGodByNameAsync(GodByNameParameter parameter)
{
    if (string.IsNullOrWhiteSpace(parameter.Name))
    {
        return new List<God>();
    }

    var query = _context.Gods.AsQueryable();

    if (parameter.IncludeAliases)
    {
        query = query.Where(g => 
            EF.Functions.Like(g.Name, $"%{parameter.Name}%") ||
            g.Aliases.Any(a => EF.Functions.Like(a.Name, $"%{parameter.Name}%"))
        );
    }
    else
    {
        query = query.Where(g => EF.Functions.Like(g.Name, $"%{parameter.Name}%"));
    }

    return await query
        .Include(g => g.Aliases)
        .ToListAsync();
}
```

### 4.5 Add Input Validation

Create a validation filter or use FluentValidation:

```csharp
// Install package: dotnet add package FluentValidation.AspNetCore

// Create validator
public class GodInputValidator : AbstractValidator<GodInput>
{
    public GodInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("God name is required")
            .MaximumLength(100).WithMessage("God name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.MythologyId)
            .GreaterThan(0).WithMessage("MythologyId must be a positive integer");

        When(x => x.Id.HasValue, () =>
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("If provided, Id must be a positive integer");
        });
    }
}
```

Register validators in Program.cs:

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<GodInputValidator>();
```

### 4.6 Add Resilience Policies

Consider adding Polly for resilience patterns:

```csharp
// Install package: dotnet add package Microsoft.Extensions.Http.Polly

// Add retry and circuit breaker policies for database operations
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(connectionString);
})
.AddTransient<IRetryPolicy>(provider => 
    Policy.Handle<SqliteException>()
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
);
```

### 4.7 Enhance Logging

Update exception logging to capture relevant context:

```csharp
catch (Exception ex)
{
    logger.LogError(ex, 
        "Error processing request {RequestPath} with parameters {@Parameters}",
        context.Request.Path,
        new { id, includeAliases });
    throw;
}
```

---

## 5. References

### OWASP Resources
- [OWASP Top 10 2025 - A10: Mishandling of Exceptional Conditions](https://owasp.org/Top10/A10_2025-Mishandling_of_Exceptional_Conditions/)
- [OWASP Improper Error Handling](https://owasp.org/www-community/Improper_Error_Handling)
- [OWASP Error Handling Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Error_Handling_Cheat_Sheet.html)

### Microsoft Documentation
- [Handle errors in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
- [Problem Details for HTTP APIs (RFC 7807)](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails)
- [Exception handling in ASP.NET Core Web APIs](https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors)
- [EF Core Query Filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)
- [SQL injection prevention in Entity Framework](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries#passing-parameters)

### Security Standards
- [CWE-209: Information Exposure Through Error Messages](https://cwe.mitre.org/data/definitions/209.html)
- [CWE-89: SQL Injection](https://cwe.mitre.org/data/definitions/89.html)
- [CWE-755: Improper Handling of Exceptional Conditions](https://cwe.mitre.org/data/definitions/755.html)
- [RFC 7807: Problem Details for HTTP APIs](https://datatracker.ietf.org/doc/html/rfc7807)

### Additional Resources
- [Polly Resilience Framework](https://github.com/App-vNext/Polly)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [Serilog Structured Logging](https://serilog.net/)
- [.NET Exception Handling Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

---

**Next Steps:**
1. Implement global exception handling middleware with ProblemDetails
2. Refactor repository methods to use `FirstOrDefaultAsync` and return nullable types
3. Update all endpoints to handle null returns and validate input parameters
4. Replace raw SQL with parameterized LINQ queries to prevent SQL injection
5. Add comprehensive validation using FluentValidation
6. Implement structured logging for all exception scenarios
7. Add integration tests to verify proper error responses and status codes
8. Update API documentation to reflect error response formats
