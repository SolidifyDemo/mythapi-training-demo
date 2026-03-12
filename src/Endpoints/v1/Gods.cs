using Microsoft.AspNetCore.Authorization;
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
        gods.MapGet("{id}", GetGodById);
        gods.MapGet("search/{name}", SearchGodsByName);
        gods.MapPost("", AddOrUpdateGods).RequireAuthorization("Admin");
        gods.MapDelete("", DeleteAllGods).RequireAuthorization("Admin");
        gods.MapDelete("{id}", DeleteGodById).RequireAuthorization("Admin");
    }

    public static Task<IList<God>> GetAlllGods(IGodRepository repository) => repository.GetAllGodsAsync();

    public static async Task<IResult> GetGodById(int id, IGodRepository repository)
    {
        if (id <= 0) return TypedResults.BadRequest("Invalid ID: must be greater than zero.");
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

    public static async Task<IResult> SearchGodsByName(string name, IGodRepository repository, [FromQuery] bool includeAliases = false)
    {
        if (string.IsNullOrWhiteSpace(name)) return TypedResults.BadRequest("Name cannot be empty.");
        var gods = await repository.GetGodByNameAsync(new GodByNameParameter(name, includeAliases));
        return TypedResults.Ok(gods);
    }

    public static async Task<IResult> AddOrUpdateGods(List<GodInput>? gods, IGodRepository repository)
    {
        if (gods == null || gods.Count == 0) return TypedResults.BadRequest("No gods provided.");
        var result = await repository.AddOrUpdateGods(gods);
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> DeleteAllGods(IGodRepository repository)
    {
        await repository.DeleteAllGodsAsync();
        return TypedResults.NoContent();
    }

    public static async Task<IResult> DeleteGodById(int id, IGodRepository repository)
    {
        if (id <= 0) return TypedResults.BadRequest("Invalid ID: must be greater than zero.");
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