# OWASP A08:2025 - Software and Data Integrity Failures

**Severity:** High  
**OWASP Category:** A08:2025 - Software and Data Integrity Failures  
**Affected Files:**
- `src/Gods/Models/God.cs` (GodInput model)
- `src/Common/Database/Models/God.cs` (God entity)
- `src/Common/Database/Models/Alias.cs` (Alias entity)
- `src/Common/Database/Models/Mythology.cs` (Mythology entity)
- `src/Common/Database/AppDBContext.cs` (database configuration)
- `src/Gods/DBRepositories/GodRepository.cs` (data persistence)
- `src/Endpoints/v1/Gods.cs` (API endpoint)

---

## 1. Description

The MythApi application accepts user input without proper validation or integrity controls before persisting data to the database. String fields in the `GodInput` model (`Name`, `Description`) are unbounded and lack validation constraints, allowing arbitrary content of unlimited length to be stored. This violates data integrity principles and exposes the application to data corruption, storage exhaustion, and potential security vulnerabilities.

The application does not enforce:
- Maximum length constraints on string inputs
- Required field validation at the API boundary
- Format or pattern validation
- Data sanitization before persistence
- Database-level integrity constraints beyond foreign key relationships

---

## 2. Details

### 2.1 Unvalidated Input Model

The `GodInput` model in `src/Gods/Models/God.cs` accepts unbounded string inputs:

```csharp
public class GodInput {
    public int? Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int MythologyId { get; set; }
}
```

**Issues:**
- No `[Required]` attributes despite null-forgiving operators
- No `[MaxLength]` or `[StringLength]` constraints
- No `[RegularExpression]` for format validation
- No custom validation logic

### 2.2 Missing Database Constraints

The database entity models and EF Core configuration lack length constraints:

**God Entity** (`src/Common/Database/Models/God.cs`):
```csharp
public class God {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int MythologyId { get; set; }
    public List<Alias> Aliases { get; set; } = [];
}
```

**AppDbContext Configuration** (`src/Common/Database/AppDBContext.cs`):
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<Mythology>().ToTable("Mythology");
    modelBuilder.Entity<God>().ToTable("God");
    modelBuilder.Entity<Alias>().ToTable("Alias");
    // ... foreign key relationships only, no string length constraints
}
```

The `OnModelCreating` method only configures table names and foreign key relationships. No `HasMaxLength()` constraints are defined.

### 2.3 Direct Persistence Without Validation

The `GodRepository.AddOrUpdateGods` method in `src/Gods/DBRepositories/GodRepository.cs` directly persists input without validation:

```csharp
public async Task<List<God>> AddOrUpdateGods(List<GodInput> gods) {
    foreach(var god in gods) {
        if (god.Id.HasValue && _context.Gods.Any(x => x.Id == god.Id)) {
            _context.Gods.Where(x => x.Id == god.Id)
                .ExecuteUpdate(setter => 
                    setter.SetProperty(x => x.Name, god.Name)
                        .SetProperty(x => x.Description, god.Description));
        } else {
            var newGod = new God {
                Name = god.Name,
                MythologyId = god.MythologyId,
                Description = god.Description
            };
            _context.Gods.Add(newGod);
        }
    }
    await _context.SaveChangesAsync();
    return await _context.Gods.ToListAsync();
}
```

Values from `GodInput` are directly assigned to the `God` entity without any validation checks.

### 2.4 Lack of CI/CD Integrity Enforcement

While the project has workflow files in `.github/workflows/`, there are no apparent validation rules or schema checks enforced during the CI pipeline to prevent invalid data structures or missing validation attributes from being deployed.

---

## 3. Implications

### 3.1 Storage Exhaustion
Attackers can submit extremely large strings (megabytes or gigabytes) for `Name` or `Description` fields, potentially:
- Exhausting database storage
- Causing out-of-memory errors
- Degrading query performance
- Increasing backup sizes and costs

### 3.2 Data Integrity Violations
Without validation:
- Empty strings could be stored for required fields
- Inconsistent data formats make analysis difficult
- Null or whitespace-only values bypass intent
- Database state becomes unreliable

### 3.3 Application Instability
Large payloads can cause:
- Increased memory consumption during serialization/deserialization
- Network transmission delays
- Timeout errors in downstream services
- Denial of Service (DoS) conditions

### 3.4 Security Boundary Weakness
Lack of input validation weakens the security posture:
- Enables injection attacks if data is used in dynamic queries (already present in SQL injection vulnerability)
- Allows malicious content to propagate through the system
- Makes it harder to implement proper security controls later

### 3.5 Compliance and Audit Failures
Many compliance frameworks (PCI-DSS, GDPR, HIPAA) require:
- Input validation at all system boundaries
- Data integrity controls
- Audit trails of data modifications

Unbounded inputs without validation violate these requirements.

---

## 4. Implementation of Fix

### 4.1 Add Data Annotations to Input Models

Update `src/Gods/Models/God.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace MythApi.Gods.Models;

public class GodInput {
    public int? Id { get; set; }

    [Required(ErrorMessage = "God name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-\u0080-\uFFFF]+$", ErrorMessage = "Name contains invalid characters")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "God description is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "MythologyId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "MythologyId must be a positive integer")]
    public int MythologyId { get; set; }
}
```

### 4.2 Add FluentValidation (Alternative/Additional Approach)

Install FluentValidation package:
```bash
dotnet add package FluentValidation.AspNetCore
```

Create `src/Gods/Validators/GodInputValidator.cs`:

```csharp
using FluentValidation;
using MythApi.Gods.Models;

namespace MythApi.Gods.Validators;

public class GodInputValidator : AbstractValidator<GodInput> {
    public GodInputValidator() {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("God name is required")
            .Length(1, 200).WithMessage("Name must be between 1 and 200 characters")
            .Matches(@"^[a-zA-Z0-9\s\-\u0080-\uFFFF]+$").WithMessage("Name contains invalid characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .Length(10, 2000).WithMessage("Description must be between 10 and 2000 characters");

        RuleFor(x => x.MythologyId)
            .GreaterThan(0).WithMessage("MythologyId must be a positive integer");
    }
}
```

Register FluentValidation in `src/Program.cs`:

```csharp
using FluentValidation;
using MythApi.Gods.Validators;

// Add to service registration section
builder.Services.AddValidatorsFromAssemblyContaining<GodInputValidator>();
builder.Services.AddFluentValidationAutoValidation();
```

### 4.3 Add Database-Level Constraints

Update `src/Common/Database/AppDBContext.cs`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder) {
    // Map entities to tables
    modelBuilder.Entity<Mythology>().ToTable("Mythology");
    modelBuilder.Entity<God>().ToTable("God");
    modelBuilder.Entity<Alias>().ToTable("Alias");
    
    // Configure God entity constraints
    modelBuilder.Entity<God>()
        .Property(g => g.Name)
        .IsRequired()
        .HasMaxLength(200);
    
    modelBuilder.Entity<God>()
        .Property(g => g.Description)
        .IsRequired()
        .HasMaxLength(2000);
    
    // Configure Alias entity constraints
    modelBuilder.Entity<Alias>()
        .Property(a => a.Name)
        .IsRequired()
        .HasMaxLength(200);
    
    // Configure Mythology entity constraints
    modelBuilder.Entity<Mythology>()
        .Property(m => m.Name)
        .IsRequired()
        .HasMaxLength(100);
    
    // Existing foreign key relationships
    modelBuilder.Entity<God>()
        .HasMany(e => e.Aliases)
        .WithOne()
        .HasForeignKey(e => e.GodId)
        .IsRequired();
    
    modelBuilder.Entity<Mythology>()
        .HasMany(e => e.Gods)
        .WithOne()
        .HasForeignKey(e => e.MythologyId)
        .IsRequired();

    base.OnModelCreating(modelBuilder);
}
```

Generate and apply migration:
```bash
dotnet ef migrations add AddStringLengthConstraints
dotnet ef database update
```

### 4.4 Add Explicit Validation in Repository (Defense in Depth)

Update `src/Gods/DBRepositories/GodRepository.cs` to add manual validation checks:

```csharp
public async Task<List<God>> AddOrUpdateGods(List<GodInput> gods) {
    // Validate input before processing
    foreach (var god in gods) {
        if (string.IsNullOrWhiteSpace(god.Name) || god.Name.Length > 200) {
            throw new ArgumentException($"Invalid name for god: {god.Name}");
        }
        if (string.IsNullOrWhiteSpace(god.Description) || god.Description.Length > 2000) {
            throw new ArgumentException($"Invalid description for god with name: {god.Name}");
        }
        if (god.MythologyId <= 0) {
            throw new ArgumentException($"Invalid MythologyId: {god.MythologyId}");
        }
    }

    foreach(var god in gods) {
        if (god.Id.HasValue && _context.Gods.Any(x => x.Id == god.Id)) {
            _context.Gods.Where(x => x.Id == god.Id)
                .ExecuteUpdate(setter => 
                    setter.SetProperty(x => x.Name, god.Name)
                        .SetProperty(x => x.Description, god.Description));
        } else {
            var newGod = new God {
                Name = god.Name,
                MythologyId = god.MythologyId,
                Description = god.Description
            };
            _context.Gods.Add(newGod);
        }
    }

    await _context.SaveChangesAsync();
    return await _context.Gods.ToListAsync();
}
```

### 4.5 Add Validation Tests

Create `tests/UnitTests/GodInputValidationTests.cs`:

```csharp
using Xunit;
using FluentValidation.TestHelper;
using MythApi.Gods.Models;
using MythApi.Gods.Validators;

namespace UnitTests;

public class GodInputValidationTests {
    private readonly GodInputValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty() {
        var model = new GodInput { Name = "", Description = "Valid description", MythologyId = 1 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength() {
        var model = new GodInput { 
            Name = new string('A', 201), 
            Description = "Valid description", 
            MythologyId = 1 
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Too_Short() {
        var model = new GodInput { Name = "Zeus", Description = "Short", MythologyId = 1 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid() {
        var model = new GodInput { 
            Name = "Zeus", 
            Description = "King of the gods in Greek mythology", 
            MythologyId = 1 
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
```

### 4.6 CI/CD Integrity Checks

Add validation checks to `.github/workflows/test.yml`:

```yaml
      - name: Build
        run: dotnet build --configuration Release

      - name: Run Unit Tests
        run: dotnet test tests/UnitTests/UnitTests.csproj --configuration Release --no-build --verbosity normal

      - name: Run Integration Tests
        run: dotnet test tests/IntegrationTests/IntegrationTests.csproj --configuration Release --no-build --verbosity normal
      
      # Add validation check
      - name: Verify Data Annotations
        run: |
          echo "Checking for data validation attributes..."
          grep -r "StringLength\|Required\|Range" src/*/Models/ || (echo "Warning: Limited validation attributes found" && exit 0)
```

Consider adding:
- Static analysis tools (e.g., SonarQube, Roslyn analyzers)
- Security scanning (e.g., OWASP Dependency-Check)
- Database migration review process
- Pre-commit hooks for validation checks

### 4.7 Enable Automatic Model Validation

Ensure automatic validation is enabled in `src/Program.cs`:

```csharp
// This is enabled by default, but make it explicit
builder.Services.Configure<ApiBehaviorOptions>(options => {
    options.SuppressModelStateInvalidFilter = false; // Ensure validation errors return 400
});
```

### 4.8 Add Global Exception Handler for Validation Errors

Create `src/Common/Middleware/ValidationExceptionHandler.cs`:

```csharp
using Microsoft.AspNetCore.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace MythApi.Common.Middleware;

public class ValidationExceptionHandler : IExceptionHandler {
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken) {
        
        if (exception is ValidationException validationException) {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Validation Error",
                status = 400,
                detail = validationException.Message
            }, cancellationToken);
            return true;
        }

        if (exception is ArgumentException argumentException) {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Invalid Input",
                status = 400,
                detail = argumentException.Message
            }, cancellationToken);
            return true;
        }

        return false;
    }
}
```

Register in `src/Program.cs`:

```csharp
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
```

---

## 5. References

### OWASP Resources
- **OWASP Top 10 2025 - A08: Software and Data Integrity Failures**  
  https://owasp.org/Top10/A08_2025-Software_and_Data_Integrity_Failures/

- **OWASP Input Validation Cheat Sheet**  
  https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html

- **OWASP Data Validation**  
  https://owasp.org/www-community/vulnerabilities/Improper_Data_Validation

### Microsoft Documentation
- **Model validation in ASP.NET Core**  
  https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation

- **Data Annotations**  
  https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations

- **Entity Framework Core: Configuring Maximum Length**  
  https://docs.microsoft.com/en-us/ef/core/modeling/entity-properties#maximum-length

- **FluentValidation for .NET**  
  https://docs.fluentvalidation.net/

### Security Standards
- **CWE-20: Improper Input Validation**  
  https://cwe.mitre.org/data/definitions/20.html

- **CWE-129: Improper Validation of Array Index**  
  https://cwe.mitre.org/data/definitions/129.html

- **CWE-502: Deserialization of Untrusted Data**  
  https://cwe.mitre.org/data/definitions/502.html

### Best Practices
- **NIST SP 800-53: Input Validation (SI-10)**  
  https://nvlpubs.nist.gov/nistpubs/SpecialPublications/NIST.SP.800-53r5.pdf

- **ISO 27001: A.14.2.1 Secure Development Policy**  
  Input validation as part of secure development lifecycle

- **PCI DSS Requirement 6.5.1**  
  Protect applications from injection flaws through input validation
