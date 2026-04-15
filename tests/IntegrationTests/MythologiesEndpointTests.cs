using System.Net;
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
    public async Task GetMythologyByGod_WithValidGodId_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/mythologies/god/1");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task GetMythologyByGod_WithValidGodId_ShouldReturnCorrectMythology()
    {
        // Act - God ID 1 is Odin, who belongs to Norse mythology
        var mythology = await _httpClient.GetFromJsonAsync<Mythology>("/api/v1/mythologies/god/1");

        // Assert
        Assert.That(mythology, Is.Not.Null);
        Assert.That(mythology!.Name, Is.EqualTo("Norse"));
    }

    [Test]
    public async Task GetMythologyByGod_WithInvalidGodId_ShouldReturnNotFound()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/mythologies/god/99999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
