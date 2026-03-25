using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MythApi.Gods.Interfaces;
using MythApi.Common.Database.Models;
using MythApi.Gods.Models;

namespace MythApi.Endpoints.v1;
public static class Gods {

    private const int MaxBatchSize = 100;

    /// <summary>
    /// Registers all god-related API endpoints under the /api/v1/gods route group.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder used to map HTTP endpoints to handlers.</param>
    /// <remarks>
    /// <b>Endpoints registered:</b>
    /// <list type="bullet">
    /// <item><description><b>GET</b> /api/v1/gods - Retrieves all gods</description></item>
    /// <item><description><b>GET</b> /api/v1/gods/{id} - Retrieves a specific god by ID</description></item>
    /// <item><description><b>GET</b> /api/v1/gods/search/{name}?includeAliases=bool - Searches for gods by name, optionally including aliases</description></item>
    /// <item><description><b>POST</b> /api/v1/gods - Adds or updates gods (max batch size: 100)</description></item>
    /// <item><description><b>DELETE</b> /api/v1/gods/{id} - Deletes a specific god by ID</description></item>
    /// </list>
    /// <para>
    /// Each endpoint is compatible with Swagger/OpenAPI and will be included in the generated API documentation.
    /// </para>
    /// </remarks>
    public static void RegisterGodEndpoints(this IEndpointRouteBuilder endpoints) {
        
        var gods = endpoints.MapGroup("/api/v1/gods");

        gods.MapGet("", GetAlllGods);
        gods.MapGet("{id}", GetGodById);
        gods.MapGet("search/{name}", SearchGodsByName);
        gods.MapPost("", AddOrUpdateGods);
        gods.MapDelete("{id}", DeleteGodById);
    }

    /// <summary>
    /// Adds new gods or updates existing gods in the database.
    /// </summary>
    /// <remarks>
    /// <b>HTTP POST</b> /api/v1/gods
    /// 
    /// Accepts a list of god input objects. If a god's Id is provided and exists, the god is updated; otherwise, a new god is created.
    /// The list must contain at least one item and no more than 100 items.
    /// </remarks>
    /// <param name="gods">A list of <see cref="GodInput"/> objects. Each object should include Name, Description, and MythologyId. Optionally include Id to update an existing god.</param>
    /// <param name="repository">The repository instance used to persist god data to the database.</param>
    /// <returns>
    /// <see cref="Ok{T}"/> (200) with the list of added or updated gods, or
    /// <see cref="BadRequest{T}"/> (400) if the input list is empty or exceeds the maximum batch size.
    /// </returns>
    /// <response code="200">Returns the list of added or updated gods</response>
    /// <response code="400">If the input list is empty or exceeds the maximum batch size of 100</response>
    public static async Task<IResult> AddOrUpdateGods(List<GodInput> gods, IGodRepository repository)
    {
        if (gods == null || gods.Count == 0)
            return TypedResults.BadRequest("The gods list must not be empty.");

        if (gods.Count > MaxBatchSize)
            return TypedResults.BadRequest($"The gods list must not exceed {MaxBatchSize} items.");

        var result = await repository.AddOrUpdateGods(gods);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Retrieves all gods from the database.
    /// </summary>
    /// <remarks>
    /// <b>HTTP GET</b> /api/v1/gods
    /// 
    /// Returns a complete list of all gods across all mythologies.
    /// </remarks>
    /// <param name="repository">The repository instance used to query god data from the database.</param>
    /// <returns>A list containing all gods stored in the database, including their associated mythologies and aliases.</returns>
    /// <response code="200">Returns the list of all gods</response>
    public static Task<IList<God>> GetAlllGods(IGodRepository repository) => repository.GetAllGodsAsync();

    /// <summary>
    /// Retrieves a single god by its unique identifier.
    /// </summary>
    /// <remarks>
    /// <b>HTTP GET</b> /api/v1/gods/{id}
    ///
    /// Returns the god whose <c>Id</c> matches the provided path parameter.
    /// Returns <b>400 Bad Request</b> when the ID is not a positive integer.
    /// Returns <b>404 Not Found</b> when no god with the given ID exists.
    /// </remarks>
    /// <param name="id">The unique integer identifier of the god to retrieve. Must be greater than zero.</param>
    /// <param name="repository">The repository instance used to query god data from the database.</param>
    /// <returns>
    /// <see cref="Ok{God}"/> (200) with the matched <see cref="God"/> object, or
    /// <see cref="BadRequest{T}"/> (400) when the ID is invalid, or
    /// <see cref="NotFound"/> (404) when no matching god is found.
    /// </returns>
    /// <response code="200">Returns the god with the specified ID</response>
    /// <response code="400">The provided ID is not a positive integer</response>
    /// <response code="404">No god was found with the given ID</response>
    public static async Task<IResult> GetGodById(int id, IGodRepository repository)
    {
        if (id <= 0)
            return TypedResults.BadRequest("The god ID must be a positive integer.");

        var god = await repository.GetGodAsync(new GodParameter(id));
        return god is null ? TypedResults.NotFound() : TypedResults.Ok(god);
    }

    /// <summary>
    /// Searches for gods by name, optionally including matches from aliases.
    /// </summary>
    /// <remarks>
    /// <b>HTTP GET</b> /api/v1/gods/search/{name}?includeAliases=bool
    ///
    /// Performs a partial name search. When <paramref name="includeAliases"/> is <c>true</c>,
    /// gods whose aliases match the search term are also returned.
    /// Returns <b>400 Bad Request</b> when the name is empty or whitespace.
    /// </remarks>
    /// <param name="name">The name (or partial name) to search for. Must not be empty or whitespace.</param>
    /// <param name="repository">The repository instance used to query god data from the database.</param>
    /// <param name="includeAliases">When <c>true</c>, also searches alias names. Defaults to <c>false</c>.</param>
    /// <returns>
    /// <see cref="Ok{T}"/> (200) with the list of matching gods, or
    /// <see cref="BadRequest{T}"/> (400) when the name is empty or whitespace.
    /// </returns>
    /// <response code="200">Returns the list of matching gods</response>
    /// <response code="400">The provided name is empty or whitespace</response>
    public static async Task<IResult> SearchGodsByName(string name, IGodRepository repository, [FromQuery] bool includeAliases = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            return TypedResults.BadRequest("The search name must not be empty or whitespace.");

        var result = await repository.GetGodByNameAsync(new GodByNameParameter(name, includeAliases));
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Deletes a single god by its unique identifier.
    /// </summary>
    /// <remarks>
    /// <b>HTTP DELETE</b> /api/v1/gods/{id}
    ///
    /// Removes the god record with the specified ID from the database.
    /// Returns <b>400 Bad Request</b> when the ID is not a positive integer.
    /// Returns <b>404 Not Found</b> when no god with the given ID exists.
    /// Returns <b>204 No Content</b> on success.
    /// </remarks>
    /// <param name="id">The unique integer identifier of the god to delete. Must be greater than zero.</param>
    /// <param name="repository">The repository instance used to delete the god record from the database.</param>
    /// <returns>
    /// <see cref="NoContent"/> (204) on success, or
    /// <see cref="BadRequest{T}"/> (400) when the ID is invalid, or
    /// <see cref="NotFound"/> (404) when no matching god is found.
    /// </returns>
    /// <response code="204">The god was successfully deleted. No content is returned.</response>
    /// <response code="400">The provided ID is not a positive integer.</response>
    /// <response code="404">No god was found with the given ID.</response>
    public static async Task<IResult> DeleteGodById(int id, IGodRepository repository)
    {
        if (id <= 0)
            return TypedResults.BadRequest("The god ID must be a positive integer.");

        try
        {
            await repository.DeleteGodByIdAsync(new GodParameter(id));
            return TypedResults.NoContent();
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound();
        }
    }
}