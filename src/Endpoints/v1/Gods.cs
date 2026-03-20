using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MythApi.Gods.Interfaces;
using MythApi.Common.Database.Models;
using MythApi.Gods.Models;
using Serilog;

namespace MythApi.Endpoints.v1;
public static class Gods {
    private const int MaxBatchSize = 100;

    public static void RegisterGodEndpoints(this IEndpointRouteBuilder endpoints) {
        
        var gods = endpoints.MapGroup("/api/v1/gods");

        gods.MapGet("", GetAlllGods);
        gods.MapGet("{id}", GetGodById);
        gods.MapGet("search/{name}", SearchGodsByName);
        gods.MapPost("", AddOrUpdateGods);
        gods.MapDelete("", DeleteAllGods);
        gods.MapDelete("{id}", DeleteGodById);
    }

    public static async Task<IResult> AddOrUpdateGods(List<GodInput> gods, IGodRepository repository)
    {
        if (gods == null || gods.Count == 0)
        {
            Log.Warning("AddOrUpdateGods called with empty list");
            return TypedResults.BadRequest("At least one god must be provided.");
        }

        if (gods.Count > MaxBatchSize)
        {
            Log.Warning("AddOrUpdateGods called with {Count} gods, exceeding max batch size {MaxBatchSize}", gods.Count, MaxBatchSize);
            return TypedResults.BadRequest($"Batch size cannot exceed {MaxBatchSize} gods.");
        }

        var result = await repository.AddOrUpdateGods(gods);
        return TypedResults.Ok(result);
    }

    public static Task<IList<God>> GetAlllGods(IGodRepository repository) => repository.GetAllGodsAsync();

    public static async Task<IResult> GetGodById(int id, IGodRepository repository)
    {
        if (id <= 0)
        {
            Log.Warning("GetGodById called with invalid id {Id}", id);
            return TypedResults.BadRequest("Id must be a positive integer.");
        }

        try
        {
            var god = await repository.GetGodAsync(new GodParameter(id));
            return TypedResults.Ok(god);
        }
        catch (InvalidOperationException)
        {
            Log.Warning("God with id {Id} not found", id);
            return TypedResults.NotFound();
        }
    }

    public static async Task<IResult> SearchGodsByName(string name, IGodRepository repository, [FromQuery] bool includeAliases = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Log.Warning("SearchGodsByName called with empty name");
            return TypedResults.BadRequest("Name must not be empty.");
        }

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
        {
            Log.Warning("DeleteGodById called with invalid id {Id}", id);
            return TypedResults.BadRequest("Id must be a positive integer.");
        }

        try
        {
            await repository.DeleteGodByIdAsync(new GodParameter(id));
            return TypedResults.NoContent();
        }
        catch (InvalidOperationException)
        {
            Log.Warning("God with id {Id} not found for deletion", id);
            return TypedResults.NotFound();
        }
    }
}