using Microsoft.AspNetCore.Http.HttpResults;
using MythApi.Common.Database.Models;
using MythApi.Mythologies.Interfaces;

public static class Mythologies
{
    public static void RegisterMythologiesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var mythologies = endpoints.MapGroup("/api/v1/mythologies");

        mythologies.MapGet("", GetAllMythologies);
        mythologies.MapGet("god/{godId}", GetMythologyByGod);
    }

    public static Task<IList<Mythology>> GetAllMythologies(IMythologyRepository repository) => repository.GetAllMythologiesAsync();

    public static async Task<Results<Ok<Mythology>, NotFound>> GetMythologyByGod(int godId, IMythologyRepository repository)
    {
        var mythology = await repository.GetMythologyByGodIdAsync(godId);
        return mythology is null ? TypedResults.NotFound() : TypedResults.Ok(mythology);
    }
}
