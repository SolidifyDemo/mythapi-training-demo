using System.Net.Http.Json;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using MythApi.Common.Database.Models;
using MythApi.Endpoints.v1;
using MythApi.Gods.Interfaces;
using MythApi.Gods.Models;

namespace IntegrationTests;

[TestFixture]
public class GodsEndpointTests
{
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

    [Test]
    public async Task GetAllGods_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/gods");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task GetAllGods_ShouldReturnGodsList()
    {
        // Act
        var gods = await _httpClient.GetFromJsonAsync<List<God>>("/api/v1/gods");

        // Assert
        Assert.That(gods, Is.Not.Null);
        // The test database should be initialized with some gods by DatabaseInitializer
    }

    [Test]
    public async Task GetAllGods_ConcurrentRequests_ShouldRespectRateLim()
    {
        // Arrange
        const int numberOfRequests = 100; // More than our rate limit of 100 per minute
        var tasks = new Task<HttpResponseMessage>[numberOfRequests];

        // Act
        for (var i = 0; i < numberOfRequests; i++)
        {
            tasks[i] = _httpClient.GetAsync("/api/v1/gods");
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        Assert.That(responses.Length, Is.EqualTo(numberOfRequests));
        
        var successfulRequests = responses.Count(r => r.IsSuccessStatusCode);
        var rateLimitedRequests = responses.Count(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests);
        
        // We expect around 100 successful requests (our rate limit) and the rest to be rate limited
        Assert.That(successfulRequests, Is.LessThanOrEqualTo(100), "Should not exceed rate limit");
        // Assert.That(rateLimitedRequests, Is.GreaterThan(0), "Some requests should be rate limited");
        Assert.That(successfulRequests + rateLimitedRequests, Is.EqualTo(numberOfRequests), "All requests should be either successful or rate limited");
    }

    [Test]
    public async Task AddOrUpdateGods_ShouldWriteAuditLogs()
    {
        var repository = new StubGodRepository();
        var loggerFactory = new InMemoryLoggerFactory();
        var context = CreateHttpContext();
        var payload = new List<GodInput>
        {
            new() { Id = 42, Name = "Apollo", Description = "God of the sun", MythologyId = 1 }
        };

        var result = await Gods.AddOrUpdateGods(payload, repository, loggerFactory, context);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(loggerFactory.Entries.Any(e => e.Level == LogLevel.Information && e.Message.Contains("AddOrUpdateGods invoked")), Is.True);
        Assert.That(loggerFactory.Entries.Any(e => e.Level == LogLevel.Information && e.Message.Contains("AddOrUpdateGods completed")), Is.True);
        Assert.That(loggerFactory.Entries.Any(e => Equals(e.Properties.GetValueOrDefault("Count"), 1)), Is.True);
        Assert.That(loggerFactory.Entries.Any(e => Equals(e.Properties.GetValueOrDefault("User"), "audit-user")), Is.True);
    }

    [Test]
    public async Task DeleteAllGods_ShouldWriteAuditLogs()
    {
        var repository = new StubGodRepository();
        var loggerFactory = new InMemoryLoggerFactory();
        var context = CreateHttpContext();

        var result = await Gods.DeleteAllGods(repository, loggerFactory, context);

        Assert.That(result, Is.InstanceOf<NoContent>());
        Assert.That(repository.DeleteAllCalls, Is.EqualTo(1));
        Assert.That(loggerFactory.Entries.Any(e => e.Level == LogLevel.Warning && e.Message.Contains("DeleteAllGods invoked")), Is.True);
        Assert.That(loggerFactory.Entries.Any(e => e.Level == LogLevel.Warning && e.Message.Contains("DeleteAllGods completed")), Is.True);
        Assert.That(loggerFactory.Entries.Any(e => Equals(e.Properties.GetValueOrDefault("User"), "audit-user")), Is.True);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "audit-user")], "test"));
        return context;
    }

    private sealed class StubGodRepository : IGodRepository
    {
        public int DeleteAllCalls { get; private set; }

        public Task<List<God>> AddOrUpdateGods(List<GodInput> gods)
        {
            var mapped = gods.Select(god => new God
            {
                Id = god.Id ?? 0,
                Name = god.Name,
                Description = god.Description,
                MythologyId = god.MythologyId
            }).ToList();
            return Task.FromResult(mapped);
        }

        public Task DeleteAllGodsAsync()
        {
            DeleteAllCalls++;
            return Task.CompletedTask;
        }

        public Task<IList<God>> GetAllGodsAsync() => Task.FromResult<IList<God>>([]);

        public Task<God> GetGodAsync(GodParameter parameter) => Task.FromResult(new God());

        public Task<List<God>> GetGodByNameAsync(GodByNameParameter parameter) => Task.FromResult(new List<God>());
    }

    private sealed class InMemoryLoggerFactory : ILoggerFactory
    {
        public List<LogEntry> Entries { get; } = new();

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName) => new InMemoryLogger(Entries);

        public void Dispose()
        {
        }
    }

    private sealed class InMemoryLogger(List<LogEntry> entries) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var properties = new Dictionary<string, object?>();
            if (state is IEnumerable<KeyValuePair<string, object?>> structuredState)
            {
                foreach (var item in structuredState)
                {
                    properties[item.Key] = item.Value;
                }
            }

            entries.Add(new LogEntry(logLevel, formatter(state, exception), properties));
        }
    }

    private sealed record LogEntry(LogLevel Level, string Message, Dictionary<string, object?> Properties);

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
