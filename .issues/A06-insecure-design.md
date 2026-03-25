# A06:2025 Insecure Design - Destructive Operations and Missing Constraints

**Severity:** High  
**OWASP Category:** A06:2025 Insecure Design  
**Status:** Open  
**Date Identified:** March 20, 2026

**Affected Files:**
- [src/Endpoints/v1/Gods.cs](../src/Endpoints/v1/Gods.cs)
- [src/Gods/Models/God.cs](../src/Gods/Models/God.cs)
- [src/Gods/Interfaces/IGodRepository.cs](../src/Gods/Interfaces/IGodRepository.cs)

---

## 1. Description

The MythApi codebase exhibits multiple instances of **insecure design** that fail to adequately protect against misuse, resource exhaustion, and accidental data loss. These design flaws violate the principle of secure-by-default and expose the API to both intentional abuse and accidental destructive operations.

The most critical finding is the `DeleteAllGods` endpoint, which provides a publicly accessible, unauthenticated mechanism to irreversibly delete all god records from the database without any confirmation, rate limiting, or administrative safeguards. Additionally, the API lacks fundamental protective constraints such as pagination, input validation, and bulk operation limits.

---

## 2. Details

### 2.1 Destructive DeleteAllGods Endpoint

**Location:** [src/Endpoints/v1/Gods.cs](../src/Endpoints/v1/Gods.cs#L80-L85)

```csharp
public static async Task<IResult> DeleteAllGods(IGodRepository repository)
{
    await repository.DeleteAllGodsAsync();
    return Results.NoContent();
}
```

**Issues:**
- Exposed as `HTTP DELETE /api/v1/gods` with no authentication or authorization
- No confirmation mechanism (e.g., confirmation token, "are you sure?" pattern)
- No admin-only restriction or role-based access control
- No audit logging of who performed the destructive operation
- Irreversible hard delete with no soft-delete option
- No rate limiting to prevent accidental rapid deletion

### 2.2 No Pagination on GetAllGods

**Location:** [src/Endpoints/v1/Gods.cs](../src/Endpoints/v1/Gods.cs#L67)

```csharp
public static Task<IList<God>> GetAlllGods(IGodRepository repository) => 
    repository.GetAllGodsAsync();
```

**Issues:**
- Returns entire dataset in a single response, risking memory exhaustion
- No `skip`/`take` or `page`/`pageSize` parameters
- No maximum result limit enforcement
- Can cause performance degradation and timeouts with large datasets
- Vulnerable to denial-of-service through repeated large data requests

### 2.3 Missing Input Constraints on GodInput

**Location:** [src/Gods/Models/God.cs](../src/Gods/Models/God.cs#L3-L11)

```csharp
public class GodInput {
    public int? Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int MythologyId { get; set; }
}
```

**Issues:**
- No `[MaxLength]` or `[StringLength]` attributes on `Name` and `Description`
- No `[Required]` validation attributes (relies only on null-forgiving operator)
- Allows arbitrarily large strings that could fill database storage or memory
- No regex pattern validation for `Name` (e.g., preventing special SQL characters)
- No range validation on `MythologyId` to ensure valid foreign key

### 2.4 No Safeguards for Bulk Operations

**Location:** [src/Endpoints/v1/Gods.cs](../src/Endpoints/v1/Gods.cs#L52)

```csharp
public static Task<List<God>> AddOrUpdateGods(List<GodInput> gods, IGodRepository repository) => 
    repository.AddOrUpdateGods(gods);
```

**Issues:**
- No maximum limit on list size for `AddOrUpdateGods`
- Could accept thousands of records in a single POST request
- No transaction size validation or chunking guidance
- Risks database connection timeouts and lock contention

---

## 3. Implications

### 3.1 Data Loss and Integrity Risks
- **Accidental deletion:** A misrouted HTTP DELETE request or automated script could irreversibly delete all gods
- **No recovery mechanism:** Without soft deletes or audit trails, data recovery requires database backups
- **Business continuity impact:** Complete data loss would render the API unusable until restored

### 3.2 Resource Exhaustion and Denial of Service
- **Memory overflow:** Requesting all gods could exhaust server memory if dataset grows large
- **Database overload:** Unpaginated queries place unnecessary load on the database
- **Slow response times:** Large result sets increase latency and reduce API responsiveness
- **Bulk insert abuse:** Attackers could submit massive lists of gods to overwhelm the database

### 3.3 Input Validation Bypass
- **Database storage abuse:** Unbounded strings could fill database storage with junk data
- **SQL injection potential:** Without proper constraints, malicious inputs are more likely to slip through
- **Data quality degradation:** No validation allows nonsensical or malformed data into the system

### 3.4 Compliance and Audit Failures
- **No audit trail:** Destructive operations leave no record of who performed them or when
- **GDPR/compliance risk:** Some regulations require audit logs for data deletion
- **Incident investigation:** Impossible to determine root cause of data loss without logging

---

## 4. Implementation of Fix

### 4.1 Secure DeleteAllGods Endpoint

**Option A: Remove the endpoint entirely**
```csharp
// Delete this endpoint from Gods.cs
// gods.MapDelete("", DeleteAllGods); // REMOVE THIS LINE
```

**Option B: Implement soft delete with admin confirmation**

```csharp
// Update GodInput model to support soft deletes
public class God {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int MythologyId { get; set; }
    public bool IsDeleted { get; set; } = false; // Add this
    public DateTime? DeletedAt { get; set; }      // Add this
}

// Replace DeleteAllGods with admin-only soft delete:
public static async Task<IResult> DeleteAllGods(
    IGodRepository repository,
    [FromHeader(Name = "X-Confirmation-Token")] string confirmationToken,
    HttpContext context)
{
    // Verify admin role (requires authentication middleware)
    if (!context.User.IsInRole("Admin"))
        return Results.Forbid();
    
    // Verify confirmation token
    const string EXPECTED_TOKEN = "DELETE_ALL_CONFIRM_2026";
    if (confirmationToken != EXPECTED_TOKEN)
        return Results.BadRequest(new { 
            error = "Missing or invalid confirmation token",
            hint = "Include X-Confirmation-Token header with correct value"
        });
    
    // Perform soft delete
    await repository.SoftDeleteAllGodsAsync();
    
    // Log the operation
    var username = context.User.Identity?.Name ?? "Unknown";
    // Log to audit system: $"User {username} soft-deleted all gods at {DateTime.UtcNow}"
    
    return Results.NoContent();
}
```

### 4.2 Add Pagination to GetAllGods

```csharp
// Update to include pagination parameters
public static async Task<IResult> GetAlllGods(
    IGodRepository repository,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50)
{
    // Enforce limits
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 50;
    if (pageSize > 200) pageSize = 200; // Maximum 200 per page
    
    var skip = (page - 1) * pageSize;
    var gods = await repository.GetAllGodsAsync(skip, pageSize);
    var totalCount = await repository.GetGodsCountAsync();
    
    return Results.Ok(new {
        data = gods,
        pagination = new {
            page,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        }
    });
}
```

**Update repository interface:**
```csharp
public interface IGodRepository {
    Task<IList<God>> GetAllGodsAsync(int skip, int take);
    Task<int> GetGodsCountAsync();
    // ... other methods
}
```

### 4.3 Add Input Validation to GodInput

```csharp
using System.ComponentModel.DataAnnotations;

public class GodInput {
    public int? Id { get; set; }

    [Required(ErrorMessage = "God name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters")]
    [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Name can only contain letters, spaces, hyphens, apostrophes, and periods")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
    public string Description { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "MythologyId must be a positive integer")]
    public int MythologyId { get; set; }
}
```

**Enable automatic validation in Program.cs:**
```csharp
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options => {
        options.InvalidModelStateResponseFactory = context => {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => new {
                    Field = e.Key,
                    Errors = e.Value.Errors.Select(x => x.ErrorMessage).ToList()
                }).ToList();
            return new BadRequestObjectResult(new { errors });
        };
    });
```

### 4.4 Add Bulk Operation Limits

```csharp
public static async Task<IResult> AddOrUpdateGods(
    List<GodInput> gods, 
    IGodRepository repository)
{
    // Enforce maximum batch size
    const int MAX_BATCH_SIZE = 100;
    if (gods == null || gods.Count == 0)
        return Results.BadRequest(new { error = "Request body must contain at least one god" });
    
    if (gods.Count > MAX_BATCH_SIZE)
        return Results.BadRequest(new { 
            error = $"Batch size exceeds maximum of {MAX_BATCH_SIZE} gods",
            hint = "Split your request into multiple batches"
        });
    
    var result = await repository.AddOrUpdateGods(gods);
    return Results.Ok(result);
}
```

---

## 5. References

### OWASP Resources
- [OWASP Top 10 2025 - A06:2025 Insecure Design](https://owasp.org/Top10/A06_2021-Insecure_Design/)
- [OWASP API Security Top 10 - API4:2023 Unrestricted Resource Consumption](https://owasp.org/API-Security/editions/2023/en/0xa4-unrestricted-resource-consumption/)
- [OWASP API Security Top 10 - API5:2023 Broken Function Level Authorization](https://owasp.org/API-Security/editions/2023/en/0xa5-broken-function-level-authorization/)
- [OWASP Cheat Sheet: Input Validation](https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html)
- [OWASP Cheat Sheet: Mass Assignment](https://cheatsheetseries.owasp.org/cheatsheets/Mass_Assignment_Cheat_Sheet.html)

### Microsoft Documentation
- [ASP.NET Core Model Validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)
- [ASP.NET Core Data Annotations](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
- [Implementing Pagination in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/sort-filter-page)
- [ASP.NET Core Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction)

### Best Practices
- **Principle of Least Privilege**: Destructive operations should require elevated permissions
- **Defense in Depth**: Implement multiple layers of protection (auth, confirmation, rate limiting, audit)
- **Secure by Default**: APIs should default to safe behavior (pagination enabled, size limits enforced)
- **Soft Deletes**: Implement logical deletion with `IsDeleted` flags rather than hard deletes for critical data
- **Audit Logging**: All destructive operations must be logged with user, timestamp, and scope

### Additional Reading
- [The Twelve-Factor App - Configuration](https://12factor.net/config) - For managing confirmation tokens
- [REST API Design Best Practices](https://stackoverflow.blog/2020/03/02/best-practices-for-rest-api-design/)
- [Pagination in REST APIs](https://www.moesif.com/blog/technical/api-design/REST-API-Design-Filtering-Sorting-and-Pagination/)

---

**Next Steps:**
1. Prioritize removal or securing of `DeleteAllGods` endpoint (CRITICAL)
2. Implement pagination on `GetAllGods` (HIGH)
3. Add validation attributes to `GodInput` model (HIGH)
4. Add batch size limits to `AddOrUpdateGods` (MEDIUM)
5. Implement comprehensive audit logging (MEDIUM)
6. Add integration tests covering these security scenarios (MEDIUM)
