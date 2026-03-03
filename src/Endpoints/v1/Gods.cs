using Microsoft.AspNetCore.Mvc;
using MythApi.Gods.Interfaces;
using MythApi.Common.Database.Models;
using MythApi.Gods.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Serilog;

namespace MythApi.Endpoints.v1;
public static class Gods {
    public static void RegisterGodEndpoints(this IEndpointRouteBuilder endpoints) {
        
        var gods = endpoints.MapGroup("/api/v1/gods");


        gods.MapGet("", GetAlllGods);
        gods.MapGet("{id}", GetGodById);
        gods.MapGet("search/{name}", (string name, IGodRepository repository, [FromQuery] bool includeAliases = false) => repository.GetGodByNameAsync(new GodByNameParameter(name, includeAliases)));
        gods.MapPost("", AddOrUpdateGods);
    }

    public static async Task<Results<Ok<God>, NotFound, BadRequest<string>>> GetGodById(int id, IGodRepository repository)
    {
        try
        {
            if (id <= 0)
            {
                Log.Warning("GetGodById called with invalid id: {Id}", id);
                return TypedResults.BadRequest("Invalid god ID. ID must be greater than 0.");
            }

            var god = await repository.GetGodAsync(new GodParameter(id));
            
            if (god == null)
            {
                Log.Warning("God not found with id: {Id}", id);
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(god);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving god with id: {Id}", id);
            throw;
        }
    }

    public static Task<List<God>> AddOrUpdateGods(List<GodInput> gods, IGodRepository repository) => repository.AddOrUpdateGods(gods);

    public static Task<IList<God>> GetAlllGods(IGodRepository repository) => repository.GetAllGodsAsync();
}