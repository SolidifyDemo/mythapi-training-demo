using Microsoft.AspNetCore.Http.HttpResults;
using MythApi.Common.Database.Models;
using MythApi.Mythologies.Interfaces;

public static class Mythologies
{
    public static void RegisterMythologiesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var mythologies = endpoints.MapGroup("/api/v1/mythologies");

        mythologies.MapGet("", GetAllMythologies);
        mythologies.MapGet("{id}", GetMythologyById);
        mythologies.MapDelete("{id}", DeleteMythologyById);
    }

    public static Task<IList<Mythology>> GetAllMythologies(IMythologyRepository repository) => repository.GetAllMythologiesAsync();

    public static async Task<IResult> GetMythologyById(int id, IMythologyRepository repository)
    {
        var mythology = await repository.GetMythologyByIdAsync(id);
        return mythology is null ? TypedResults.NotFound() : TypedResults.Ok(mythology);
    }

    /// <summary>
    /// Permanently deletes a mythology by its unique identifier.
    /// </summary>
    /// <remarks>
    /// <para>HTTP DELETE: <c>/api/v1/mythologies/{id}</c></para>
    /// <para>Deletes the mythology with the specified <paramref name="id"/>. Returns 204 No Content if successful, or 404 Not Found if the mythology does not exist.</para>
    /// </remarks>
    /// <param name="id">The unique identifier of the mythology to delete. Must be a valid integer corresponding to an existing mythology.</param>
    /// <param name="repository">The repository instance used to access and modify mythology data. Injected by the framework.</param>
    /// <returns>
    /// <para><see cref="TypedResults.NoContent"/> (204) if the mythology was deleted successfully.</para>
    /// <para><see cref="TypedResults.NotFound"/> (404) if no mythology with the given <paramref name="id"/> exists.</para>
    /// </returns>
    /// <response code="204">The mythology was successfully deleted.</response>
    /// <response code="404">No mythology was found with the given ID.</response>
    public static async Task<IResult> DeleteMythologyById(int id, IMythologyRepository repository)
    {
        var deleted = await repository.DeleteMythologyByIdAsync(id);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
