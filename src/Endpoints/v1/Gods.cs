using Microsoft.AspNetCore.Mvc;
using MythApi.Gods.Interfaces;
using MythApi.Common.Database.Models;
using MythApi.Gods.Models;

namespace MythApi.Endpoints.v1;
public static class Gods {
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 100;

    public static void RegisterGodEndpoints(this IEndpointRouteBuilder endpoints) {
        
        var gods = endpoints.MapGroup("/api/v1/gods")
            .RequireRateLimiting("api");


        gods.MapGet("", (IGodRepository repository, [FromQuery] int page = DefaultPage, [FromQuery] int pageSize = DefaultPageSize) => GetAllGods(page, pageSize, repository));
        gods.MapGet("{id}", (int id, IGodRepository repository) => repository.GetGodAsync(new GodParameter(id)));
        gods.MapGet("search/{name}", (string name, IGodRepository repository, [FromQuery] bool includeAliases = false) => repository.GetGodByNameAsync(new GodByNameParameter(name, includeAliases)));
        gods.MapPost("", AddOrUpdateGods);
        gods.MapDelete("", DeleteAllGods).RequireAuthorization("admin");
    }

    public static Task<List<God>> AddOrUpdateGods(List<GodInput> gods, IGodRepository repository) => repository.AddOrUpdateGods(gods);

    public static Task<IList<God>> GetAllGods(IGodRepository repository) => GetAllGods(DefaultPage, DefaultPageSize, repository);

    public static Task<IList<God>> GetAllGods(int page, int pageSize, IGodRepository repository)
    {
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        return repository.GetAllGodsAsync(safePage, safePageSize);
    }

    public static async Task<IResult> DeleteAllGods(IGodRepository repository)
    {
        await repository.DeleteAllGodsAsync();
        return Results.NoContent();
    }
}
