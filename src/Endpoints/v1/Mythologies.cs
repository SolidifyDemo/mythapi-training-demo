using Microsoft.AspNetCore.Http.HttpResults;
using MythApi.Common.Database.Models;
using MythApi.Mythologies.Interfaces;

public static class Mythologies
{
    /// <summary>
    /// Registers all mythology-related API endpoints under the /api/v1/mythologies route group.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder used to map HTTP endpoints to handlers.</param>
    /// <remarks>
    /// <b>Endpoints registered:</b>
    /// <list type="bullet">
    /// <item><description><b>GET</b> /api/v1/mythologies - Retrieves all mythologies</description></item>
    /// <item><description><b>GET</b> /api/v1/mythologies/{id} - Retrieves a single mythology by ID</description></item>
    /// </list>
    /// <para>
    /// Each endpoint is compatible with Swagger/OpenAPI and will be included in the generated API documentation.
    /// </para>
    /// </remarks>
    public static void RegisterMythologiesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var mythologies = endpoints.MapGroup("/api/v1/mythologies");

        mythologies.MapGet("", GetAllMythologies);
        mythologies.MapGet("{id}", GetMythologyById);
        mythologies.MapGet("search/by-god", GetMythologyByGodName);
    }

    /// <summary>
    /// Retrieves all mythologies from the database.
    /// </summary>
    /// <remarks>
    /// <b>HTTP GET</b> /api/v1/mythologies
    ///
    /// Returns a complete list of all mythologies.
    /// </remarks>
    /// <param name="repository">The repository instance used to query mythology data from the database.</param>
    /// <returns>A list containing all mythologies stored in the database, including their associated gods.</returns>
    /// <response code="200">Returns the list of all mythologies</response>
    public static Task<IList<Mythology>> GetAllMythologies(IMythologyRepository repository) => repository.GetAllMythologiesAsync();

    /// <summary>
    /// Retrieves a single mythology by its unique identifier.
    /// </summary>
    /// <remarks>
    /// <b>HTTP GET</b> /api/v1/mythologies/{id}
    ///
    /// Returns the mythology whose <c>Id</c> matches the provided path parameter.
    /// Returns <b>404 Not Found</b> when no mythology with the given ID exists.
    /// </remarks>
    /// <param name="id">The unique integer identifier of the mythology to retrieve.</param>
    /// <param name="repository">The repository instance used to query mythology data from the database.</param>
    /// <returns>
    /// <see cref="Ok{Mythology}"/> (200) with the matched <see cref="Mythology"/> object, or
    /// <see cref="NotFound"/> (404) when no matching mythology is found.
    /// </returns>
    /// <response code="200">Returns the mythology with the specified ID</response>
    /// <response code="404">No mythology was found with the given ID</response>
    public static async Task<IResult> GetMythologyById(int id, IMythologyRepository repository)
    {
        var mythology = await repository.GetMythologyByIdAsync(id);
        return mythology is null ? TypedResults.NotFound() : TypedResults.Ok(mythology);
    }

    /// <summary>
    /// Retrieves a mythology by the name of one of its associated gods.
    /// </summary>
    /// <remarks>
    /// <b>HTTP GET</b> /api/v1/mythologies/search/by-god
    ///
    /// Searches for a god with the specified name (case-insensitive) and returns the mythology to which that god belongs.
    /// Returns the complete mythology object including all associated gods.
    /// Returns <b>404 Not Found</b> when no god with the given name exists.
    /// </remarks>
    /// <param name="name">The name of the god to search for (case-insensitive).</param>
    /// <param name="repository">The repository instance used to query mythology data from the database.</param>
    /// <returns>
    /// <see cref="Ok{Mythology}"/> (200) with the matched <see cref="Mythology"/> object and all its associated gods, or
    /// <see cref="NotFound"/> (404) when no god with the specified name is found.
    /// </returns>
    /// <response code="200">Returns the mythology containing the god with the specified name</response>
    /// <response code="404">No god was found with the given name</response>
    public static async Task<IResult> GetMythologyByGodName(string name, IMythologyRepository repository)
    {
        var mythology = await repository.GetMythologyByGodNameAsync(name);
        return mythology is null ? TypedResults.NotFound() : TypedResults.Ok(mythology);
    }
}
