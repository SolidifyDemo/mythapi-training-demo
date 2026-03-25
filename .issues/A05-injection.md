# OWASP A05:2025 - Injection Vulnerability

**Severity:** Critical  
**OWASP Category:** A05:2025 - Injection  
**Affected Files:**
- `src/Gods/DBRepositories/GodRepository.cs` (Line 58)
- `src/Endpoints/v1/Gods.cs` (Line 31)

---

## 1. Description

The `GetGodByNameAsync` method in `GodRepository` constructs SQL queries using string interpolation with unsanitized user input. This creates a critical SQL injection vulnerability that allows attackers to execute arbitrary SQL commands against the database.

The vulnerable endpoint `/api/v1/gods/search/{name}` accepts user input through the URL path parameter `name`, which is directly embedded into raw SQL queries without parameterization or sanitization.

## 2. Details

**Vulnerable Code:**

```csharp
// src/Gods/DBRepositories/GodRepository.cs (Lines 56-62)
public Task<List<God>> GetGodByNameAsync(GodByNameParameter parameter)
{
    var query = parameter.IncludeAliases 
        ? $"SELECT * FROM God WHERE Name LIKE '%{parameter.Name}%' or Id in (SELECT GodId FROM Alias WHERE Name LIKE '%{parameter.Name}%')" 
        : $"SELECT * FROM God WHERE Name LIKE '%{parameter.Name}%'";
    var result = _context.Gods.FromSqlRaw(query).ToList();
    return Task.FromResult(result);
}
```

**Attack Vector:**

User input arrives through the endpoint defined in `src/Endpoints/v1/Gods.cs`:
```csharp
gods.MapGet("search/{name}", (string name, IGodRepository repository, [FromQuery] bool includeAliases = false) 
    => repository.GetGodByNameAsync(new GodByNameParameter(name, includeAliases)));
```

**Exploitation Scenario:**

An attacker can send malicious SQL payloads through the URL:

```
GET /api/v1/gods/search/%27%3B%20DROP%20TABLE%20God%3B--
```

Decoded: `'; DROP TABLE God;--`

This results in the following SQL being executed:
```sql
SELECT * FROM God WHERE Name LIKE '%'; DROP TABLE God;--%'
```

Other exploitation examples:
- **Data Exfiltration:** `/api/v1/gods/search/%27%20UNION%20SELECT%20*%20FROM%20Users--`
- **Privilege Escalation:** `/api/v1/gods/search/%27%3B%20UPDATE%20Users%20SET%20Role=%27Admin%27--`
- **Database Enumeration:** `/api/v1/gods/search/%27%20UNION%20SELECT%20name%20FROM%20sqlite_master--`

## 3. Implications

1. **Complete Database Compromise:** Attackers can read, modify, or delete any data in the database
2. **Data Breach:** Sensitive information from all tables can be extracted, including potentially user credentials or personal data
3. **Data Integrity Loss:** Important records (gods, mythologies, aliases) can be modified or deleted
4. **Denial of Service:** Database tables can be dropped or corrupted, rendering the API unusable
5. **Compliance Violations:** GDPR, PCI-DSS, and other regulatory frameworks mandate protection against injection attacks
6. **Lateral Movement:** If database credentials have elevated permissions, attackers may compromise the underlying system

## 4. Implementation of Fix

### Option 1: Parameterized Queries with FromSqlRaw (Recommended for raw SQL)

Replace string interpolation with SQL parameters:

```csharp
public async Task<List<God>> GetGodByNameAsync(GodByNameParameter parameter)
{
    var searchPattern = $"%{parameter.Name}%";
    
    var query = parameter.IncludeAliases 
        ? @"SELECT * FROM God WHERE Name LIKE {0} 
            OR Id IN (SELECT GodId FROM Alias WHERE Name LIKE {0})"
        : "SELECT * FROM God WHERE Name LIKE {0}";
    
    var result = await _context.Gods
        .FromSqlRaw(query, searchPattern)
        .ToListAsync();
    
    return result;
}
```

### Option 2: LINQ with EF.Functions.Like (Most Secure and Recommended)

Eliminate raw SQL entirely and use LINQ with Entity Framework:

```csharp
public async Task<List<God>> GetGodByNameAsync(GodByNameParameter parameter)
{
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

**Benefits of Option 2:**
- Type-safe and compiler-checked
- Automatically parameterized by Entity Framework
- Better performance with query optimization
- Cleaner code that's easier to maintain
- Supports eager loading of related entities

### Additional Security Measures

1. **Input Validation:** Add validation to `GodByNameParameter`:
```csharp
public record GodByNameParameter(string Name, bool IncludeAliases)
{
    public string Name { get; init; } = ValidateName(Name);
    
    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty");
        if (name.Length > 100)
            throw new ArgumentException("Name too long");
        return name;
    }
}
```

2. **Database Permissions:** Ensure the application database user has minimal required permissions (SELECT, INSERT, UPDATE only)

### Test Additions

Add the following test cases to `tests/IntegrationTests/GodsEndpointTests.cs`:

```csharp
[Fact]
public async Task SearchGods_WithSQLInjectionAttempt_ReturnsNoResults()
{
    var maliciousInput = "'; DROP TABLE God;--";
    var response = await _client.GetAsync($"/api/v1/gods/search/{Uri.EscapeDataString(maliciousInput)}");
    
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    // Verify no SQL injection occurred - table still exists
    var allGodsResponse = await _client.GetAsync("/api/v1/gods");
    allGodsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
}

[Fact]
public async Task SearchGods_WithUnionInjection_ReturnsOnlyGods()
{
    var maliciousInput = "' UNION SELECT * FROM Mythology--";
    var response = await _client.GetAsync($"/api/v1/gods/search/{Uri.EscapeDataString(maliciousInput)}");
    
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var gods = await response.Content.ReadFromJsonAsync<List<God>>();
    gods.Should().NotBeNull();
    gods.Should().AllBeOfType<God>();
}
```

## 5. References

- [OWASP Top 10 2025 - A05:2025 Injection](https://owasp.org/Top10/A05_2025-Injection/)
- [CWE-89: SQL Injection](https://cwe.mitre.org/data/definitions/89.html)
- [Microsoft Security - SQL Injection](https://learn.microsoft.com/en-us/sql/relational-databases/security/sql-injection)
- [Entity Framework Core - Raw SQL Queries](https://learn.microsoft.com/en-us/ef/core/querying/raw-sql)
- [Entity Framework Core - Query Filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)
- [OWASP SQL Injection Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/SQL_Injection_Prevention_Cheat_Sheet.html)
- [SANS CWE Top 25 Most Dangerous Software Weaknesses](https://www.sans.org/top25-software-errors/)
