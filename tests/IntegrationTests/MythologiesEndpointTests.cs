using System.Net.Http.Json;
using MythApi.Common.Database.Models;

namespace IntegrationTests;

[TestFixture]
public class MythologiesEndpointTests
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
    public async Task GetAllMythologies_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/mythologies");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task GetAllMythologies_ShouldReturnMythologiesList()
    {
        // Act
        var mythologies = await _httpClient.GetFromJsonAsync<List<Mythology>>("/api/v1/mythologies");

        // Assert
        Assert.That(mythologies, Is.Not.Null);
        // The test database should be initialized with Norse and Greek mythologies by DatabaseInitializer
        Assert.That(mythologies!.Count, Is.GreaterThan(0));
    }
}
