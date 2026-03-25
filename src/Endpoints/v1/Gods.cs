using Microsoft.AspNetCore.Mvc;
using MythApi.Gods.Interfaces;
using MythApi.Common.Database.Models;
using MythApi.Gods.Models;

namespace MythApi.Endpoints.v1;
public static class Gods {

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
    /// <item><description><b>POST</b> /api/v1/gods - Adds or updates gods</description></item>
    /// </list>
    /// <para>
    /// Each endpoint is compatible with Swagger/OpenAPI and will be included in the generated API documentation.
    /// </para>
    /// </remarks>
    public static void RegisterGodEndpoints(this IEndpointRouteBuilder endpoints) {
        
        var gods = endpoints.MapGroup("/api/v1/gods");


        gods.MapGet("", GetAlllGods);
        gods.MapGet("{id}", (int id, IGodRepository repository) => repository.GetGodAsync(new GodParameter(id)));
        gods.MapGet("search/{name}", (string name, IGodRepository repository, [FromQuery] bool includeAliases = false) => repository.GetGodByNameAsync(new GodByNameParameter(name, includeAliases)));
        gods.MapPost("", AddOrUpdateGods);
        gods.MapDelete("", DeleteAllGods);
    }

    /// <summary>
    /// Adds new gods or updates existing gods in the database.
    /// </summary>
    /// <remarks>
    /// <b>HTTP POST</b> /api/v1/gods
    /// 
    /// Accepts a list of god input objects. If a god's Id is provided and exists, the god is updated; otherwise, a new god is created.
    /// </remarks>
    /// <param name="gods">A list of <see cref="GodInput"/> objects. Each object should include Name, Description, and MythologyId. Optionally include Id to update an existing god.</param>
    /// <param name="repository">The repository instance used to persist god data to the database.</param>
    /// <returns>A list of <see cref="God"/> objects that were successfully added or updated, including their assigned IDs.</returns>
    /// <response code="200">Returns the list of added or updated gods</response>
    /// <response code="400">If the input is invalid</response>
    public static Task<List<God>> AddOrUpdateGods(List<GodInput> gods, IGodRepository repository) => repository.AddOrUpdateGods(gods);

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
    /// Deletes all gods from the database.
    /// </summary>
    /// <remarks>
    /// <b>HTTP DELETE</b> /api/v1/gods
    /// 
    /// Removes all god records from the database. This operation is irreversible and will delete all gods across all mythologies.
    /// </remarks>
    /// <param name="repository">The repository instance used to perform the deletion of all god records from the database. Must not be null.</param>
    /// <returns>A <see cref="IResult"/> indicating the outcome of the operation. Returns <b>204 No Content</b> on success.</returns>
    /// <response code="204">All gods were successfully deleted. No content is returned.</response>
    /// <response code="500">An internal server error occurred while deleting gods.</response>
    /// <remarks>
    /// <b>Swagger/OpenAPI:</b> This endpoint is documented and will appear in the generated API documentation.
    /// </remarks>
    public static async Task<IResult> DeleteAllGods(IGodRepository repository)
    {
        await repository.DeleteAllGodsAsync();
        return Results.NoContent();
    }

    
}