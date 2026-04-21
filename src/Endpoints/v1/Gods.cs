using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

    public static async Task<List<God>> AddOrUpdateGods(List<GodInput> gods, IGodRepository repository, ILoggerFactory loggerFactory, HttpContext context)
    {
        var logger = loggerFactory.CreateLogger(typeof(Gods).FullName!);
        var ids = gods.Where(god => god.Id.HasValue).Select(god => god.Id!.Value).ToArray();
        logger.LogInformation(
            "AddOrUpdateGods invoked. Count={Count} GodIds={GodIds} RemoteIp={RemoteIp} User={User}",
            gods.Count,
            ids,
            context.Connection.RemoteIpAddress,
            context.User.Identity?.Name ?? "anonymous");

        var result = await repository.AddOrUpdateGods(gods);

        logger.LogInformation(
            "AddOrUpdateGods completed. SubmittedCount={SubmittedCount} PersistedCount={PersistedCount}",
            gods.Count,
            result.Count);

        return result;
    }

    public static async Task<IResult> DeleteAllGods(IGodRepository repository, ILoggerFactory loggerFactory, HttpContext context)
    {
        var logger = loggerFactory.CreateLogger(typeof(Gods).FullName!);
        logger.LogWarning(
            "DeleteAllGods invoked. RemoteIp={RemoteIp} User={User}",
            context.Connection.RemoteIpAddress,
            context.User.Identity?.Name ?? "anonymous");

        await repository.DeleteAllGodsAsync();

        logger.LogWarning("DeleteAllGods completed. All god records removed.");
        return Results.NoContent();
    }

    public static Task<IList<God>> GetAlllGods(IGodRepository repository) => repository.GetAllGodsAsync();
}
