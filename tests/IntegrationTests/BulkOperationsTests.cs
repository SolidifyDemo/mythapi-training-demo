using System.Net;
using System.Net.Http.Json;
using MythApi.Common.Database.Models;
using MythApi.Gods.Models;

namespace IntegrationTests;

[TestFixture]
public class BulkOperationsTests
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
    public async Task BulkAddGods_ValidInput_ShouldSucceed()
    {
        // Arrange
        var godsToAdd = new List<GodInput>
        {
            new GodInput { Name = "TestGod1", Description = "Test Description 1", MythologyId = 1 },
            new GodInput { Name = "TestGod2", Description = "Test Description 2", MythologyId = 1 }
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/v1/gods", godsToAdd);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        var gods = await response.Content.ReadFromJsonAsync<List<God>>();
        Assert.That(gods, Is.Not.Null);
        Assert.That(gods!.Any(g => g.Name == "TestGod1"), Is.True);
        Assert.That(gods!.Any(g => g.Name == "TestGod2"), Is.True);
    }

    [Test]
    public async Task BulkUpdateGods_ValidInput_ShouldSucceed()
    {
        // Arrange - First add some gods
        var godsToAdd = new List<GodInput>
        {
            new GodInput { Name = "UpdateTest1", Description = "Original Description", MythologyId = 1 }
        };
        var addResponse = await _httpClient.PostAsJsonAsync("/api/v1/gods", godsToAdd);
        var addedGods = await addResponse.Content.ReadFromJsonAsync<List<God>>();
        var godToUpdate = addedGods!.First(g => g.Name == "UpdateTest1");

        // Act - Update the god
        var godsToUpdate = new List<GodInput>
        {
            new GodInput { Id = godToUpdate.Id, Name = "UpdateTest1", Description = "Updated Description", MythologyId = 1 }
        };
        var updateResponse = await _httpClient.PostAsJsonAsync("/api/v1/gods", godsToUpdate);

        // Assert
        Assert.That(updateResponse.IsSuccessStatusCode, Is.True);
        var updatedGods = await updateResponse.Content.ReadFromJsonAsync<List<God>>();
        var updatedGod = updatedGods!.First(g => g.Id == godToUpdate.Id);
        Assert.That(updatedGod.Description, Is.EqualTo("Updated Description"));
    }

    [Test]
    public async Task BulkAddOrUpdate_MixedOperations_ShouldSucceed()
    {
        // Arrange - First add a god
        var godsToAdd = new List<GodInput>
        {
            new GodInput { Name = "MixedTest1", Description = "Original", MythologyId = 1 }
        };
        var addResponse = await _httpClient.PostAsJsonAsync("/api/v1/gods", godsToAdd);
        var addedGods = await addResponse.Content.ReadFromJsonAsync<List<God>>();
        var existingGod = addedGods!.First(g => g.Name == "MixedTest1");

        // Act - Mix update and insert
        var mixedOperations = new List<GodInput>
        {
            new GodInput { Id = existingGod.Id, Name = "MixedTest1", Description = "Updated", MythologyId = 1 },
            new GodInput { Name = "MixedTest2", Description = "New God", MythologyId = 2 }
        };
        var response = await _httpClient.PostAsJsonAsync("/api/v1/gods", mixedOperations);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        var gods = await response.Content.ReadFromJsonAsync<List<God>>();
        Assert.That(gods!.Any(g => g.Id == existingGod.Id && g.Description == "Updated"), Is.True);
        Assert.That(gods!.Any(g => g.Name == "MixedTest2"), Is.True);
    }

    [Test]
    public async Task BulkAddGods_EmptyList_ShouldReturnBadRequest()
    {
        // Arrange
        var emptyList = new List<GodInput>();

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/v1/gods", emptyList);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.That(errorMessage, Does.Contain("cannot be empty"));
    }

    [Test]
    public async Task BulkAddGods_MissingName_ShouldReturnBadRequest()
    {
        // Arrange
        var godsWithMissingName = new List<GodInput>
        {
            new GodInput { Name = "", Description = "Description", MythologyId = 1 }
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/v1/gods", godsWithMissingName);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.That(errorMessage, Does.Contain("Name is required"));
    }

    [Test]
    public async Task BulkAddGods_MissingDescription_ShouldReturnBadRequest()
    {
        // Arrange
        var godsWithMissingDescription = new List<GodInput>
        {
            new GodInput { Name = "TestGod", Description = "", MythologyId = 1 }
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/v1/gods", godsWithMissingDescription);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.That(errorMessage, Does.Contain("Description is required"));
    }

    [Test]
    public async Task BulkAddGods_InvalidMythologyId_ShouldReturnBadRequest()
    {
        // Arrange
        var godsWithInvalidMythology = new List<GodInput>
        {
            new GodInput { Name = "TestGod", Description = "Description", MythologyId = 999 }
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/v1/gods", godsWithInvalidMythology);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.That(errorMessage, Does.Contain("Invalid MythologyId"));
    }

    [Test]
    public async Task BulkAddGods_ExceedsSizeLimit_ShouldReturnBadRequest()
    {
        // Arrange - Create more than 100 items
        var largeList = new List<GodInput>();
        for (int i = 0; i < 101; i++)
        {
            largeList.Add(new GodInput
            {
                Name = $"God{i}",
                Description = $"Description {i}",
                MythologyId = 1
            });
        }

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/v1/gods", largeList);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.That(errorMessage, Does.Contain("cannot exceed 100 items"));
    }

    [Test]
    public async Task BulkAddGods_PartialValidationFailure_ShouldNotCommitAny()
    {
        // Arrange - Get initial count
        var initialGods = await _httpClient.GetFromJsonAsync<List<God>>("/api/v1/gods");
        var initialCount = initialGods!.Count;

        // Create a list with one valid and one invalid item
        var mixedValidityList = new List<GodInput>
        {
            new GodInput { Name = "ValidGod", Description = "Valid Description", MythologyId = 1 },
            new GodInput { Name = "InvalidGod", Description = "Invalid Description", MythologyId = 999 }
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/v1/gods", mixedValidityList);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        
        // Verify no changes were committed (transaction rolled back)
        var finalGods = await _httpClient.GetFromJsonAsync<List<God>>("/api/v1/gods");
        Assert.That(finalGods!.Count, Is.EqualTo(initialCount), "No gods should be added when validation fails");
        Assert.That(finalGods!.Any(g => g.Name == "ValidGod"), Is.False, "Valid god should not be committed when batch fails");
    }

    [Test]
    public async Task BulkAddGods_MultipleValidationErrors_ShouldReportAll()
    {
        // Arrange
        var godsWithMultipleErrors = new List<GodInput>
        {
            new GodInput { Name = "", Description = "", MythologyId = 0 },
            new GodInput { Name = "ValidName", Description = "", MythologyId = 1 }
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/v1/gods", godsWithMultipleErrors);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var errorMessage = await response.Content.ReadAsStringAsync();
        
        // Check that multiple errors are reported
        Assert.That(errorMessage, Does.Contain("Item 0"));
        Assert.That(errorMessage, Does.Contain("Item 1"));
        Assert.That(errorMessage, Does.Contain("Name is required"));
        Assert.That(errorMessage, Does.Contain("Description is required"));
    }
}
