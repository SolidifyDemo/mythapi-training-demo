using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MythApi.Gods.Interfaces;
using MythApi.Common.Database.Models;
using MythApi.Gods.Models;

namespace MythApi.Endpoints.v1;
public static class Gods {
    public static void RegisterGodEndpoints(this IEndpointRouteBuilder endpoints) {
        
        var gods = endpoints.MapGroup("/api/v1/gods");


        gods.MapGet("", GetAlllGods);
        gods.MapGet("{id}", (int id, IGodRepository repository) => GetGodById(id, repository));
        gods.MapGet("search/{name}", (string name, IGodRepository repository, [FromQuery] bool includeAliases = false) => SearchGodsByName(name, repository, includeAliases));
        gods.MapPost("", AddOrUpdateGods);
        gods.MapDelete("", DeleteAllGods);
        gods.MapDelete("{id}", (int id, IGodRepository repository) => DeleteGodById(id, repository));
    }

    public static async Task<IResult> AddOrUpdateGods(List<GodInput> gods, IGodRepository repository)
    {
        if (gods == null || gods.Count == 0)
            return TypedResults.BadRequest("At least one god must be provided.");

        var result = await repository.AddOrUpdateGods(gods);
        return TypedResults.Ok(result);
    }

    public static Task<IList<God>> GetAlllGods(IGodRepository repository) => repository.GetAllGodsAsync();

    public static async Task<IResult> GetGodById(int id, IGodRepository repository)
    {
        if (id <= 0)
            return TypedResults.BadRequest("Id must be greater than 0.");

        try
        {
            var god = await repository.GetGodAsync(new GodParameter(id));
            return TypedResults.Ok(god);
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound();
        }
    }

    public static async Task<IResult> SearchGodsByName(string name, IGodRepository repository, bool includeAliases = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            return TypedResults.BadRequest("Name must not be empty.");

        var gods = await repository.GetGodByNameAsync(new GodByNameParameter(name, includeAliases));
        return TypedResults.Ok(gods);
    }

    public static async Task<IResult> DeleteAllGods(IGodRepository repository)
    {
        await repository.DeleteAllGodsAsync();
        return TypedResults.NoContent();
    }

    public static async Task<IResult> DeleteGodById(int id, IGodRepository repository)
    {
        if (id <= 0)
            return TypedResults.BadRequest("Id must be greater than 0.");

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