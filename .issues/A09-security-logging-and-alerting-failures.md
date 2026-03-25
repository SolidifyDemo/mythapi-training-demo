# A09:2025 Security Logging and Alerting Failures

**Severity:** High  
**OWASP Category:** A09:2021 / A09:2025 - Security Logging and Monitoring Failures  
**Status:** Open  
**Date Identified:** 2026-03-20

## Affected Files

- `src/Program.cs` - Logging configuration set to Warning minimum level
- `src/appsettings.json` - Error-level logging baseline omits security telemetry
- `src/Endpoints/v1/Gods.cs` - Destructive operations (POST, DELETE) without audit logging
- `src/Endpoints/v1/Mythologies.cs` - Read-only endpoints (lower risk but incomplete telemetry)
- `src/Common/Database/AppDBContext.cs` - Database operations lack audit trail

---

## 1. Description

The MythAPI application lacks comprehensive security logging and monitoring capabilities essential for detecting, responding to, and recovering from security incidents. Critical business operations—particularly data modification and deletion—execute without generating audit trails. The current logging configuration prioritizes error-level events while suppressing informational and warning-level telemetry that could reveal reconnaissance activities, unauthorized access attempts, or data exfiltration patterns.

Without adequate logging and alerting, security incidents may remain undetected for extended periods, making forensic analysis impossible and preventing timely incident response. The application currently violates OWASP A09:2025 guidelines by failing to log authentication events, authorization failures, input validation failures, and critical business transactions.

---

## 2. Details

### Current Logging State

#### Logging Configuration Issues

**File:** `src/Program.cs` (Lines 18-22)
```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MythApi")
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .MinimumLevel.Warning()  // ❌ Too restrictive for security monitoring
    .WriteTo.Console()
    .CreateLogger();
```

**Issue:** Minimum level set to `Warning` suppresses:
- `Information` level security events (successful authentication, authorization decisions)
- `Debug` level detailed request telemetry useful for forensic analysis
- Normal business operations that establish baseline behavior

**File:** `src/appsettings.json`
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Error",  // ❌ Only logs errors and critical events
      "Microsoft.AspNetCore": "Error",
      "Microsoft.EntityFrameworkCore": "Error",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  }
}
```

#### Missing Audit Logging on Critical Endpoints

**File:** `src/Endpoints/v1/Gods.cs`

**Unaudited Operations:**

1. **AddOrUpdateGods** (Line 52) - POST /api/v1/gods
   - No logging of which gods were created/modified
   - No user identification or correlation ID
   - No record of previous state for update operations
   - No validation failure logging

2. **DeleteAllGods** (Lines 80-84) - DELETE /api/v1/gods
   ```csharp
   public static async Task<IResult> DeleteAllGods(IGodRepository repository)
   {
       await repository.DeleteAllGodsAsync();  // ❌ No audit log before/after deletion
       return Results.NoContent();
   }
   ```
   - Catastrophic operation with zero audit trail
   - No record of what data was deleted
   - No authentication/authorization logging
   - No safeguards or confirmation requirements

#### Missing Security Telemetry

The application does not log:

- **Authentication events:** No record of login attempts, successes, or failures (if authentication is later added)
- **Authorization failures:** No logging when access is denied
- **Input validation failures:** Failed deserialization, invalid payloads, injection attempts
- **Rate limiting violations:** No evidence of potential abuse
- **Suspicious patterns:** Multiple failed requests, enumeration attempts
- **Resource access:** No record of who accessed what data and when

#### No Alerting Infrastructure

- No integration with monitoring platforms (Application Insights, Datadog, etc.)
- No alert definitions for critical events
- No automated incident response workflows
- No centralized log aggregation for distributed deployments
- Console-only logging in production (ephemeral, easily lost)

#### Missing Correlation and Context

Logs lack essential contextual metadata:
- **Correlation IDs:** Cannot trace requests across service boundaries
- **Request metadata:** IP address, user agent, request path, method
- **Session/User identification:** Cannot attribute actions to specific users
- **Timing information:** Request duration, timestamp precision
- **Structured data:** Logs are not easily queryable or parseable

---

## 3. Implications

### Security Impact

1. **Undetectable Breaches**
   - Attackers can operate undetected indefinitely
   - No evidence for forensic analysis or legal proceedings
   - Cannot determine scope or timeline of compromise

2. **Delayed Incident Response**
   - Security incidents discovered only through side effects (customer complaints, data leaks)
   - No real-time detection of ongoing attacks
   - Extended Mean Time to Detect (MTTD) and Mean Time to Respond (MTTR)

3. **Compliance Violations**
   - GDPR requires audit trails for data access and modifications
   - SOC 2 requires comprehensive logging of security-relevant events
   - PCI DSS mandates detailed logging and monitoring
   - HIPAA requires audit controls for protected health information

4. **Forensic Analysis Impossible**
   - Cannot reconstruct attack timeline
   - Cannot identify compromised accounts or data
   - Cannot determine root cause or attack vector
   - No evidence for law enforcement or insurance claims

### Business Impact

1. **Data Integrity Risks**
   - Malicious or accidental deletions unrecoverable and unattributable
   - No accountability for data modifications
   - Cannot prove data accuracy for regulatory purposes

2. **Operational Blindness**
   - Cannot detect performance degradation patterns
   - Unable to identify abusive usage or resource exhaustion
   - No visibility into application health in production

3. **Liability Exposure**
   - Regulatory fines for compliance failures
   - Legal liability for preventable security incidents
   - Reputational damage from undetected breaches

### Specific Threat Scenarios

1. **Insider Threat:** Malicious employee deletes all gods via DELETE endpoint—no record of who, when, or what
2. **Account Takeover:** Compromised credentials used to modify data—no unusual activity detected
3. **Data Exfiltration:** Attacker repeatedly queries all gods—no rate limiting alerts or access logging
4. **API Abuse:** Bot scrapes entire database—consumption patterns not monitored
5. **Privilege Escalation:** Unauthorized API access succeeds—no authorization failure logs to trigger investigation

---

## 4. Implementation of Fix

### Phase 1: Enhanced Structured Logging (Immediate)

#### Step 1.1: Update Logging Configuration

**File:** `src/Program.cs`

Replace current Serilog configuration with:

```csharp
using Serilog.Events;

// Configure Serilog with appropriate filtering and enrichment
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MythApi")
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .Enrich.WithProperty("MachineName", Environment.MachineName)
    .Enrich.WithThreadId()
    .MinimumLevel.Information()  // ✅ Changed from Warning to Information
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)  // ✅ Log SQL queries
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/mythapi-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();
```

**File:** `src/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information",
      "MythApi.Endpoints": "Information",
      "MythApi.Security": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

#### Step 1.2: Add Correlation ID Middleware

Create `src/Common/Middleware/CorrelationIdMiddleware.cs`:

```csharp
using System.Diagnostics;
using Serilog.Context;

namespace MythApi.Common.Middleware;

/// <summary>
/// Middleware that ensures every request has a unique correlation ID for distributed tracing.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Append(CorrelationIdHeader, correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("RemoteIP", context.Connection.RemoteIpAddress?.ToString()))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].ToString()))
        {
            await _next(context);
        }
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}
```

Register in `Program.cs` after `var app = builder.Build();`:

```csharp
app.UseCorrelationId();  // Add before UseSwagger()
```

#### Step 1.3: Create Audit Logging Service

Create `src/Common/Services/AuditLogger.cs`:

```csharp
using Serilog;
using ILogger = Serilog.ILogger;

namespace MythApi.Common.Services;

/// <summary>
/// Centralized audit logging service for security-relevant events.
/// </summary>
public interface IAuditLogger
{
    void LogDataModification(string operation, string entityType, object? entityId, object? before, object? after, string? userId = null);
    void LogDataDeletion(string operation, string entityType, int recordCount, object? deletedData, string? userId = null);
    void LogAuthorizationFailure(string resource, string action, string? userId = null, string? reason = null);
    void LogInputValidationFailure(string endpoint, object? invalidInput, string? errors);
    void LogSuspiciousActivity(string activityType, string details, string? ipAddress = null);
}

public class AuditLogger : IAuditLogger
{
    private readonly ILogger _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogger(IHttpContextAccessor httpContextAccessor)
    {
        _logger = Log.ForContext<AuditLogger>();
        _httpContextAccessor = httpContextAccessor;
    }

    public void LogDataModification(string operation, string entityType, object? entityId, object? before, object? after, string? userId = null)
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        _logger.Information(
            "AUDIT: Data {Operation} | Entity: {EntityType} | ID: {EntityId} | User: {UserId} | IP: {IpAddress} | CorrelationId: {CorrelationId} | Before: {@Before} | After: {@After}",
            operation,
            entityType,
            entityId,
            userId ?? "Anonymous",
            ipAddress,
            correlationId,
            before,
            after
        );
    }

    public void LogDataDeletion(string operation, string entityType, int recordCount, object? deletedData, string? userId = null)
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        _logger.Warning(
            "AUDIT: Data {Operation} | Entity: {EntityType} | RecordCount: {RecordCount} | User: {UserId} | IP: {IpAddress} | CorrelationId: {CorrelationId} | DeletedData: {@DeletedData}",
            operation,
            entityType,
            recordCount,
            userId ?? "Anonymous",
            ipAddress,
            correlationId,
            deletedData
        );
    }

    public void LogAuthorizationFailure(string resource, string action, string? userId = null, string? reason = null)
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        _logger.Warning(
            "AUDIT: Authorization Failed | Resource: {Resource} | Action: {Action} | User: {UserId} | IP: {IpAddress} | CorrelationId: {CorrelationId} | Reason: {Reason}",
            resource,
            action,
            userId ?? "Anonymous",
            ipAddress,
            correlationId,
            reason ?? "Access denied"
        );
    }

    public void LogInputValidationFailure(string endpoint, object? invalidInput, string? errors)
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        _logger.Warning(
            "AUDIT: Input Validation Failed | Endpoint: {Endpoint} | IP: {IpAddress} | CorrelationId: {CorrelationId} | Errors: {Errors} | Input: {@InvalidInput}",
            endpoint,
            ipAddress,
            correlationId,
            errors,
            invalidInput
        );
    }

    public void LogSuspiciousActivity(string activityType, string details, string? ipAddress = null)
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();
        ipAddress ??= _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        _logger.Warning(
            "SECURITY: Suspicious Activity | Type: {ActivityType} | Details: {Details} | IP: {IpAddress} | CorrelationId: {CorrelationId}",
            activityType,
            details,
            ipAddress,
            correlationId
        );
    }
}
```

Register in `Program.cs`:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();
```

#### Step 1.4: Implement Audit Logging in Endpoints

**File:** `src/Endpoints/v1/Gods.cs`

Update `DeleteAllGods`:

```csharp
public static async Task<IResult> DeleteAllGods(IGodRepository repository, IAuditLogger auditLogger)
{
    // Capture data before deletion for audit trail
    var godsToDelete = await repository.GetAllGodsAsync();
    var deletionSnapshot = new
    {
        RecordCount = godsToDelete.Count,
        Timestamp = DateTime.UtcNow,
        GodIds = godsToDelete.Select(g => g.Id).ToList(),
        GodNames = godsToDelete.Select(g => g.Name).ToList()
    };

    // Log BEFORE deletion
    auditLogger.LogDataDeletion(
        operation: "DELETE_ALL",
        entityType: "God",
        recordCount: godsToDelete.Count,
        deletedData: deletionSnapshot
    );

    try
    {
        await repository.DeleteAllGodsAsync();
        
        // Log successful completion
        Log.Information(
            "Successfully deleted all gods. RecordCount: {RecordCount}, CorrelationId: {CorrelationId}",
            godsToDelete.Count,
            auditLogger._httpContextAccessor.HttpContext?.Items["CorrelationId"]
        );

        return Results.NoContent();
    }
    catch (Exception ex)
    {
        // Log failure
        Log.Error(ex, "Failed to delete all gods. RecordCount: {RecordCount}", godsToDelete.Count);
        throw;
    }
}
```

Update `AddOrUpdateGods`:

```csharp
public static async Task<List<God>> AddOrUpdateGods(List<GodInput> gods, IGodRepository repository, IAuditLogger auditLogger)
{
    var results = new List<God>();

    foreach (var godInput in gods)
    {
        God? beforeState = null;
        string operation;

        if (godInput.Id.HasValue)
        {
            beforeState = await repository.GetGodAsync(new GodParameter(godInput.Id.Value));
            operation = beforeState != null ? "UPDATE" : "CREATE";
        }
        else
        {
            operation = "CREATE";
        }

        var result = await repository.AddOrUpdateGods(new List<GodInput> { godInput });
        var afterState = result.FirstOrDefault();

        auditLogger.LogDataModification(
            operation: operation,
            entityType: "God",
            entityId: afterState?.Id,
            before: beforeState,
            after: afterState
        );

        if (afterState != null)
        {
            results.Add(afterState);
        }
    }

    return results;
}
```

### Phase 2: Alerting and Monitoring (High Priority)

#### Step 2.1: Integrate Application Insights (Azure)

Add to `Program.cs`:

```csharp
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// Add Serilog Application Insights sink
Log.Logger = new LoggerConfiguration()
    // ... existing config ...
    .WriteTo.ApplicationInsights(
        builder.Configuration["ApplicationInsights:ConnectionString"],
        TelemetryConverter.Traces
    )
    .CreateLogger();
```

#### Step 2.2: Define Alert Rules

Create alerts for:

1. **Mass Deletion Detection**
   - Trigger: DELETE operations affecting >10 records in 5-minute window
   - Severity: Critical
   - Query:
     ```kusto
     traces
     | where message contains "AUDIT: Data DELETE"
     | where customDimensions.RecordCount > 10
     | summarize Count=count() by bin(timestamp, 5m)
     | where Count > 0
     ```

2. **Repeated Authorization Failures**
   - Trigger: >5 authorization failures from same IP in 1 minute
   - Severity: High
   - Query:
     ```kusto
     traces
     | where message contains "AUDIT: Authorization Failed"
     | summarize FailureCount=count() by RemoteIP=tostring(customDimensions.IpAddress), bin(timestamp, 1m)
     | where FailureCount > 5
     ```

3. **Input Validation Failures Spike**
   - Trigger: >20 validation failures in 5 minutes (potential injection attack)
   - Severity: Medium
   - Query:
     ```kusto
     traces
     | where message contains "AUDIT: Input Validation Failed"
     | summarize Count=count() by bin(timestamp, 5m)
     | where Count > 20
     ```

4. **Suspicious Activity Detection**
   - Trigger: Any log with "SECURITY: Suspicious Activity"
   - Severity: High
   - Immediate notification

#### Step 2.3: Create Alert Action Groups

Configure notification channels:
- Email to security team
- SMS for critical alerts
- Webhook to incident management system (PagerDuty, Opsgenie)
- Microsoft Teams channel integration

### Phase 3: Long-Term Improvements

1. **Centralized Log Management**
   - Deploy ELK stack (Elasticsearch, Logstash, Kibana) or Azure Monitor
   - Configure log retention policies (minimum 90 days)
   - Set up automated log backups to immutable storage

2. **Security Information and Event Management (SIEM)**
   - Integrate with Splunk, Azure Sentinel, or similar
   - Create correlation rules for advanced threat detection
   - Implement anomaly detection with machine learning

3. **Rate Limiting and Abuse Detection**
   - Implement per-IP rate limiting with AspNetCoreRateLimit
   - Log all rate limit violations
   - Create automatic IP blocking for severe abuse

4. **Authentication/Authorization Logging**
   - When authentication is added, log all auth events
   - Implement JWT token validation logging
   - Track privilege escalation attempts

### Testing the Implementation

Create test scenarios:

```csharp
// Test: Verify audit logging on deletion
[Fact]
public async Task DeleteAllGods_LogsAuditTrail()
{
    // Arrange
    var repository = new MockGodRepository();
    var auditLogger = new Mock<IAuditLogger>();
    await repository.AddGods(/* test data */);

    // Act
    await Gods.DeleteAllGods(repository, auditLogger.Object);

    // Assert
    auditLogger.Verify(
        x => x.LogDataDeletion(
            "DELETE_ALL",
            "God",
            It.IsAny<int>(),
            It.IsAny<object>(),
            It.IsAny<string>()
        ),
        Times.Once
    );
}
```

---

## 5. References

### OWASP Resources
- [OWASP Top 10 2021 - A09:2021 Security Logging and Monitoring Failures](https://owasp.org/Top10/A09_2021-Security_Logging_and_Monitoring_Failures/)
- [OWASP Application Security Verification Standard (ASVS) v4.0 - V7: Error Handling and Logging](https://owasp.org/www-project-application-security-verification-standard/)
- [OWASP Logging Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html)
- [OWASP AppSensor Detection Points](https://owasp.org/www-project-appsensor/)

### Microsoft Documentation
- [Logging in .NET Core and ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/)
- [Serilog in ASP.NET Core](https://github.com/serilog/serilog-aspnetcore)
- [Application Insights for ASP.NET Core](https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core)
- [Structured Logging with Serilog](https://github.com/serilog/serilog/wiki/Structured-Data)

### Compliance Standards
- **GDPR Article 30:** Records of processing activities
- **PCI DSS 10.1-10.7:** Logging and monitoring requirements
- **SOC 2 CC7.2:** System monitoring for security events
- **NIST SP 800-53 AU Family:** Audit and accountability controls
- **ISO 27001 A.12.4.1:** Event logging requirements

### Industry Best Practices
- [CIS Controls v8 - Control 8: Audit Log Management](https://www.cisecurity.org/controls/audit-log-management)
- [SANS Critical Security Controls - Logging and Monitoring](https://www.sans.org/cloud-security/critical-controls/)
- [Cloud Security Alliance - Logging and Monitoring Guidelines](https://cloudsecurityalliance.org/)

### Tools and Libraries
- [Serilog](https://serilog.net/) - Structured logging library for .NET
- [Seq](https://datalust.co/seq) - Log aggregation and analysis
- [Application Insights](https://azure.microsoft.com/en-us/services/monitor/) - Azure monitoring platform
- [AspNetCoreRateLimit](https://github.com/stefanprodan/AspNetCoreRateLimit) - Rate limiting middleware

### Related Security Issues
- A01:2021 - Broken Access Control (alerts needed for authorization failures)
- A03:2021 - Injection (input validation logging detects injection attempts)
- A04:2021 - Insecure Design (logging supports security requirements)
- A07:2021 - Identification and Authentication Failures (authentication event logging)

---

**Next Steps:**
1. Implement Phase 1 (Enhanced Structured Logging) immediately  
2. Set up Application Insights integration within 1 week  
3. Define and deploy alert rules within 2 weeks  
4. Conduct security logging review and penetration testing  
5. Document security logging runbook for incident response team  
6. Schedule quarterly reviews of log retention and alerting effectiveness
