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

    // --- GET /api/v1/mythologies ---

    [Test]
    public async Task GetAllMythologies_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/mythologies");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task GetAllMythologies_ShouldReturnNonNullList()
    {
        // Act
        var mythologies = await _httpClient.GetFromJsonAsync<List<Mythology>>("/api/v1/mythologies");

        // Assert
        Assert.That(mythologies, Is.Not.Null);
    }

    [Test]
    public async Task GetAllMythologies_ShouldReturnSeededMythologies()
    {
        // Act
        var mythologies = await _httpClient.GetFromJsonAsync<List<Mythology>>("/api/v1/mythologies");

        // Assert
        Assert.That(mythologies, Has.Count.GreaterThan(0));
    }

    // --- GET /api/v1/mythologies/{id} ---

    [Test]
    public async Task GetMythologyById_WithValidId_ShouldReturnSuccessStatusCode()
    {
        // Arrange — first fetch all to get a known valid ID
        var mythologies = await _httpClient.GetFromJsonAsync<List<Mythology>>("/api/v1/mythologies");
        var validId = mythologies!.First().Id;

        // Act
        var response = await _httpClient.GetAsync($"/api/v1/mythologies/{validId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetMythologyById_WithValidId_ShouldReturnCorrectMythology()
    {
        // Arrange
        var mythologies = await _httpClient.GetFromJsonAsync<List<Mythology>>("/api/v1/mythologies");
        var expected = mythologies!.First();

        // Act
        var result = await _httpClient.GetFromJsonAsync<Mythology>($"/api/v1/mythologies/{expected.Id}");

        // Assert
        Assert.That(result!.Name, Is.EqualTo(expected.Name));
    }

    [Test]
    public async Task GetMythologyById_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/mythologies/99999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetMythologyById_WithValidId_ShouldReturnMythologyWithId()
    {
        // Arrange
        var mythologies = await _httpClient.GetFromJsonAsync<List<Mythology>>("/api/v1/mythologies");
        var expectedId = mythologies!.First().Id;

        // Act
        var result = await _httpClient.GetFromJsonAsync<Mythology>($"/api/v1/mythologies/{expectedId}");

        // Assert
        Assert.That(result!.Id, Is.EqualTo(expectedId));
    }

    // --- GET /api/v1/mythologies/search/by-god ---

    [Test]
    public async Task GetMythologyByGodName_WithValidGodName_ShouldReturnSuccessStatusCode()
    {
        // Arrange — first fetch all mythologies and their gods to get a valid god name
        var mythologies = await _httpClient.GetFromJsonAsync<List<Mythology>>("/api/v1/mythologies");
        Assert.That(mythologies, Has.Count.GreaterThan(0), "No mythologies found in database");
        
        // Note: assuming gods are seeded in the database, we'll use a known god name
        var validGodName = "Zeus"; // Assuming Zeus is seeded as a Greek god

        // Act
        var response = await _httpClient.GetAsync($"/api/v1/mythologies/search/by-god?name={validGodName}");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task GetMythologyByGodName_WithValidGodName_ShouldReturnCorrectMythology()
    {
        // Arrange
        var validGodName = "Zeus"; // Assuming Zeus is seeded as a Greek god

        // Act
        var result = await _httpClient.GetFromJsonAsync<Mythology>($"/api/v1/mythologies/search/by-god?name={validGodName}");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.Not.Null);
    }

    [Test]
    public async Task GetMythologyByGodName_WithValidGodName_ShouldReturnMythologyWithGods()
    {
        // Arrange
        var validGodName = "Zeus"; // Assuming Zeus is seeded as a Greek god

        // Act
        var result = await _httpClient.GetFromJsonAsync<Mythology>($"/api/v1/mythologies/search/by-god?name={validGodName}");

        // Assert
        Assert.That(result!.Gods, Is.Not.Null);
        Assert.That(result!.Gods, Has.Count.GreaterThan(0));
    }

    [Test]
    public async Task GetMythologyByGodName_WithInvalidGodName_ShouldReturnNotFound()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/mythologies/search/by-god?name=NonExistentGod");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}

