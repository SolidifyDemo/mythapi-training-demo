# OWASP A07:2025 - Authentication Failures

**Severity:** High  
**OWASP Category:** A07:2025 - Identification and Authentication Failures  
**Affected Files:**
- [src/Program.cs](../src/Program.cs)
- [src/Endpoints/v1/Gods.cs](../src/Endpoints/v1/Gods.cs)
- [src/Endpoints/v1/Mythologies.cs](../src/Endpoints/v1/Mythologies.cs)

---

## 1. Description

The MythApi application lacks any authentication and authorization mechanisms, exposing all API endpoints as anonymous and publicly accessible. The application does not implement identity verification for users or systems accessing the API, and there are no authorization policies to control access to sensitive operations.

Critical operations such as creating, updating, and deleting gods and mythologies can be performed by any unauthenticated user or system without credential verification. This violates the OWASP A07:2025 category, which addresses failures in confirming user identity, authentication, and session management.

## 2. Details

### Missing Authentication Infrastructure

The [Program.cs](../src/Program.cs) file shows no authentication or authorization configuration:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ❌ No AddAuthentication() call
// ❌ No AddAuthorization() call
```

The application middleware pipeline also lacks authentication enforcement:

```csharp
app.RegisterGodEndpoints();
app.RegisterMythologiesEndpoints();
app.UseSwagger();
app.UseSwaggerUI();
// ❌ No UseAuthentication() middleware
// ❌ No UseAuthorization() middleware
app.Run();
```

### Unprotected Endpoints

All endpoints in [Gods.cs](../src/Endpoints/v1/Gods.cs) are accessible without authentication:

- **GET** `/api/v1/gods` - Anonymous read access to all gods
- **GET** `/api/v1/gods/{id}` - Anonymous read access to specific god
- **GET** `/api/v1/gods/search/{name}` - Anonymous search functionality
- **POST** `/api/v1/gods` - Anonymous create/update operations
- **DELETE** `/api/v1/gods` - Anonymous deletion of all gods (destructive operation)

Example from the current implementation:

```csharp
public static void RegisterGodEndpoints(this IEndpointRouteBuilder endpoints) {
    var gods = endpoints.MapGroup("/api/v1/gods");
    
    gods.MapGet("", GetAlllGods);
    gods.MapGet("{id}", (int id, IGodRepository repository) => ...);
    gods.MapPost("", AddOrUpdateGods);
    gods.MapDelete("", DeleteAllGods);  // ❌ Destructive operation with no auth
    // ❌ No .RequireAuthorization() calls
}
```

The same vulnerability pattern exists in the Mythologies endpoints.

### Security Weaknesses Identified

1. **No Identity Verification**: The API cannot distinguish between legitimate users, malicious actors, or automated bots
2. **No Session Management**: No mechanism exists to track or invalidate user sessions
3. **No Access Control**: All users have equal access to all operations, including destructive ones
4. **No Audit Trail**: Unable to trace which entity performed which operation
5. **No Rate Limiting Context**: Cannot implement user-specific rate limiting without authentication

## 3. Implications

### Immediate Security Risks

1. **Data Manipulation**: Any actor can create, modify, or delete mythology and god records without authorization
2. **Data Exfiltration**: Complete database contents can be extracted by any party with network access
3. **Denial of Service**: The `DELETE /api/v1/gods` endpoint allows complete data destruction by unauthenticated users
4. **Compliance Violations**: Violates GDPR, SOC 2, ISO 27001, and other regulatory requirements mandating access controls
5. **Supply Chain Attacks**: If this API integrates with other systems, compromised data could propagate

### Business Impact

- **Reputational Damage**: Security breach disclosure could damage organizational credibility
- **Legal Liability**: Lack of basic security controls could result in legal action or regulatory fines
- **Operational Disruption**: Data loss from unauthorized deletion requires recovery efforts and downtime
- **Financial Loss**: Incident response, forensic analysis, and remediation costs

### Attack Scenarios

**Scenario 1: Unauthorized Data Deletion**
```bash
# Any anonymous user can delete all gods
curl -X DELETE https://mythapi.example.com/api/v1/gods
```

**Scenario 2: Data Poisoning**
```bash
# Inject malicious or incorrect data
curl -X POST https://mythapi.example.com/api/v1/gods \
  -H "Content-Type: application/json" \
  -d '[{"name":"Malicious", "description":"Fake entry", "mythologyId":1}]'
```

**Scenario 3: Information Disclosure**
```bash
# Extract complete database contents
curl https://mythapi.example.com/api/v1/gods
curl https://mythapi.example.com/api/v1/mythologies
```

## 4. Implementation of Fix

### Step 1: Install Required NuGet Packages

Add JWT Bearer authentication support to the project:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### Step 2: Configure Authentication Services

Update [Program.cs](../src/Program.cs) to add authentication and authorization services:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ... existing services ...

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? 
                throw new InvalidOperationException("JWT Key not configured")))
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Warning("Authentication failed: {Message}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Log.Information("Token validated for user: {User}", 
                context.Principal?.Identity?.Name ?? "Unknown");
            return Task.CompletedTask;
        }
    };
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Require authentication by default
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    
    // Define role-based policies
    options.AddPolicy("ReadOnly", policy => 
        policy.RequireRole("User", "Admin"));
    
    options.AddPolicy("WriteAccess", policy => 
        policy.RequireRole("Editor", "Admin"));
    
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
});

var app = builder.Build();

// ... middleware pipeline ...
```

### Step 3: Add Authentication Middleware (CRITICAL ORDER)

Add authentication and authorization middleware in the correct order in [Program.cs](../src/Program.cs):

```csharp
var app = builder.Build();

// Initialize database (existing code)
if (inMemoryDatabase || sqliteDatabase || builder.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    // ... database initialization ...
}

// IMPORTANT: Middleware order matters!
app.UseSwagger();
app.UseSwaggerUI();

// 1. Authentication must come before Authorization
app.UseAuthentication();  // ← Validates JWT tokens, populates User claims

// 2. Authorization uses the authenticated identity
app.UseAuthorization();   // ← Enforces policies and requirements

// 3. Endpoints are registered after auth middleware
app.RegisterGodEndpoints();
app.RegisterMythologiesEndpoints();

app.Run();
```

### Step 4: Protect Endpoints with Authorization

Update [Gods.cs](../src/Endpoints/v1/Gods.cs) to require authentication:

```csharp
using Microsoft.AspNetCore.Authorization;

public static void RegisterGodEndpoints(this IEndpointRouteBuilder endpoints) 
{
    var gods = endpoints.MapGroup("/api/v1/gods")
        .RequireAuthorization("ReadOnly"); // Default policy for entire group
    
    // Read operations - requires authentication (ReadOnly policy)
    gods.MapGet("", GetAlllGods)
        .WithName("GetAllGods")
        .WithOpenApi();
    
    gods.MapGet("{id}", (int id, IGodRepository repository) => 
            repository.GetGodAsync(new GodParameter(id)))
        .WithName("GetGodById")
        .WithOpenApi();
    
    gods.MapGet("search/{name}", (string name, IGodRepository repository, 
            [FromQuery] bool includeAliases = false) => 
            repository.GetGodByNameAsync(new GodByNameParameter(name, includeAliases)))
        .WithName("SearchGodByName")
        .WithOpenApi();
    
    // Write operations - requires elevated permissions
    gods.MapPost("", AddOrUpdateGods)
        .RequireAuthorization("WriteAccess")
        .WithName("AddOrUpdateGods")
        .WithOpenApi();
    
    // Destructive operations - requires admin role
    gods.MapDelete("", DeleteAllGods)
        .RequireAuthorization("AdminOnly")
        .WithName("DeleteAllGods")
        .WithOpenApi();
}
```

Apply the same pattern to [Mythologies.cs](../src/Endpoints/v1/Mythologies.cs):

```csharp
public static void RegisterMythologiesEndpoints(this IEndpointRouteBuilder endpoints)
{
    var mythologies = endpoints.MapGroup("/api/v1/mythologies")
        .RequireAuthorization("ReadOnly");
    
    mythologies.MapGet("", GetAllMythologies)
        .WithName("GetAllMythologies")
        .WithOpenApi();
    
    mythologies.MapGet("{id}", GetMythologyById)
        .WithName("GetMythologyById")
        .WithOpenApi();
    
    mythologies.MapPost("", AddOrUpdateMythologies)
        .RequireAuthorization("WriteAccess")
        .WithName("AddOrUpdateMythologies")
        .WithOpenApi();
}
```

### Step 5: Configure Application Settings

Add JWT configuration to [appsettings.json](../src/appsettings.json):

```json
{
  "Jwt": {
    "Key": "your-secret-key-min-32-characters-long-for-security",
    "Issuer": "https://mythapi.example.com",
    "Audience": "https://mythapi.example.com",
    "ExpiryMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.Authentication": "Information"
    }
  }
}
```

For development, override in [appsettings.Development.json](../src/appsettings.Development.json):

```json
{
  "Jwt": {
    "Key": "development-secret-key-minimum-32-characters-required",
    "Issuer": "https://localhost:5001",
    "Audience": "https://localhost:5001",
    "ExpiryMinutes": 120
  }
}
```

**⚠️ IMPORTANT:** Store the JWT secret key in Azure Key Vault or environment variables for production deployments. Never commit production secrets to source control.

### Step 6: Update Swagger/OpenAPI Configuration

Configure Swagger to support JWT authentication in [Program.cs](../src/Program.cs):

```csharp
using Microsoft.OpenApi.Models;

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MythApi",
        Version = "v1",
        Description = "API for managing mythologies and gods"
    });
    
    // Define JWT Bearer security scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

### Step 7: Create Token Generation Endpoint (Optional but Recommended)

For development and testing, create a token generation endpoint:

```csharp
// In Program.cs, after app initialization
app.MapPost("/api/auth/token", 
    [AllowAnonymous] (TokenRequest request, IConfiguration config) =>
{
    // ⚠️ This is a simplified example for development only
    // Production should integrate with a proper identity provider (Entra ID, Auth0, etc.)
    
    if (request.Username == "demo" && request.Password == "demo123")
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("sub", request.Username),
            new Claim("jti", Guid.NewGuid().ToString())
        };
        
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(config["Jwt:ExpiryMinutes"] ?? "60")),
            signingCredentials: credentials
        );
        
        return Results.Ok(new TokenResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = token.ValidTo
        });
    }
    
    return Results.Unauthorized();
})
.WithName("GenerateToken")
.AllowAnonymous()
.WithOpenApi();

record TokenRequest(string Username, string Password);
record TokenResponse
{
    public required string Token { get; init; }
    public DateTime ExpiresAt { get; init; }
}
```

### Step 8: Verification and Testing

**Test authentication is enforced:**

```bash
# 1. Attempt to access protected endpoint without token (should fail with 401)
curl -i https://localhost:5001/api/v1/gods

# 2. Generate a token (development endpoint)
curl -X POST https://localhost:5001/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"demo","password":"demo123"}'

# 3. Use token to access protected endpoint (should succeed with 200)
curl -i https://localhost:5001/api/v1/gods \
  -H "Authorization: Bearer eyJhbGc..."

# 4. Test role-based access control
# Try DELETE without Admin role (should fail with 403)
curl -X DELETE https://localhost:5001/api/v1/gods \
  -H "Authorization: Bearer <user-token>"
  
# Try DELETE with Admin role (should succeed with 204)
curl -X DELETE https://localhost:5001/api/v1/gods \
  -H "Authorization: Bearer <admin-token>"
```

**Verify middleware order:**
- Authentication middleware before authorization middleware
- Both auth middlewares before endpoint registration
- Check logs for authentication events

**Test policy enforcement:**
- Verify ReadOnly policy allows GET requests
- Verify WriteAccess policy required for POST requests
- Verify AdminOnly policy required for DELETE requests

### Additional Security Recommendations

1. **Implement Token Refresh Mechanism**: Short-lived access tokens with refresh tokens
2. **Add Token Revocation**: Implement a token blacklist or use reference tokens
3. **Enable HTTPS Only**: Enforce TLS for all API communications
4. **Implement Rate Limiting**: Add user-specific rate limiting middleware
5. **Add Audit Logging**: Log all authentication attempts and authorization decisions
6. **Use Claims-Based Authorization**: Implement fine-grained permissions using custom claims
7. **Integration with Identity Provider**: Replace demo token endpoint with Entra ID, Auth0, or Keycloak

## 5. References

### OWASP Resources
- [OWASP Top 10 2021 - A07:2021 Identification and Authentication Failures](https://owasp.org/Top10/A07_2021-Identification_and_Authentication_Failures/)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [OWASP JSON Web Token Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/JSON_Web_Token_for_Java_Cheat_Sheet.html)

### Microsoft Documentation
- [Authentication and authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [JWT bearer authentication in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [Policy-based authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)
- [Secure authentication flows in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization)
- [Minimal API security](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security)

### Security Standards
- [RFC 7519 - JSON Web Token (JWT)](https://datatracker.ietf.org/doc/html/rfc7519)
- [RFC 6749 - OAuth 2.0 Authorization Framework](https://datatracker.ietf.org/doc/html/rfc6749)
- [NIST SP 800-63B - Digital Identity Guidelines](https://pages.nist.gov/800-63-3/sp800-63b.html)

### Industry Best Practices
- [CWE-287: Improper Authentication](https://cwe.mitre.org/data/definitions/287.html)
- [MITRE ATT&CK - Valid Accounts (T1078)](https://attack.mitre.org/techniques/T1078/)
