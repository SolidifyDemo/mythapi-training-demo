using Microsoft.AspNetCore.Mvc;
using MythApi.Gods.Interfaces;
using MythApi.Common.Database.Models;
using MythApi.Gods.Models;

namespace MythApi.Endpoints.v1;
public static class Gods {
    public static void RegisterGodEndpoints(this IEndpointRouteBuilder endpoints) {
        
        var gods = endpoints.MapGroup("/api/v1/gods");


        gods.MapGet("", GetAlllGods);
        gods.MapGet("{id}", (int id, IGodRepository repository) => repository.GetGodAsync(new GodParameter(id)));
        gods.MapGet("search/{name}", (string name, IGodRepository repository, [FromQuery] bool includeAliases = false) => repository.GetGodByNameAsync(new GodByNameParameter(name, includeAliases)));
        gods.MapPost("", AddOrUpdateGods);
        gods.MapDelete("", DeleteAllGods);
    }

    public static async Task<IResult> AddOrUpdateGods(List<GodInput> gods, IGodRepository repository, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(nameof(Gods));
        try
        {
            var result = await repository.AddOrUpdateGods(gods);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add or update gods");
            return Results.Problem("An error occurred while saving gods.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    public static Task<IList<God>> GetAlllGods(IGodRepository repository) => repository.GetAllGodsAsync();

    public static async Task<IResult> DeleteAllGods(IGodRepository repository, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(nameof(Gods));
        try
        {
            await repository.DeleteAllGodsAsync();
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete all gods");
            return Results.Problem("An error occurred while deleting gods.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
