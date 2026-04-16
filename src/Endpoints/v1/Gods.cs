using Microsoft.AspNetCore.Mvc;
using MythApi.Gods.Interfaces;
using MythApi.Common.Database.Models;
using MythApi.Gods.Models;
using Microsoft.Extensions.Logging;

namespace MythApi.Endpoints.v1;
public static class Gods {
    public static void RegisterGodEndpoints(this IEndpointRouteBuilder endpoints) {
        
        var gods = endpoints.MapGroup("/api/v1/gods");


        gods.MapGet("", GetAllGods);
        gods.MapGet("{id}", GetGodById);
        gods.MapGet("search/{name}", SearchGodsByName);
        gods.MapPost("", AddOrUpdateGods);
        gods.MapDelete("", DeleteAllGods);
        gods.MapDelete("{id}", DeleteGodById);
    }

    public static async Task<IResult> AddOrUpdateGods(List<GodInput> gods, IGodRepository repository, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("Gods");
        if (gods is null || gods.Count == 0)
        {
            logger.LogWarning("AddOrUpdateGods called with an empty payload.");
            return TypedResults.BadRequest("At least one god is required.");
        }

        logger.LogInformation("AddOrUpdateGods called with {Count} item(s).", gods.Count);
        try
        {
            var result = await repository.AddOrUpdateGods(gods);
            logger.LogInformation("AddOrUpdateGods completed successfully with {Count} persisted god(s).", result.Count);
            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AddOrUpdateGods failed.");
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public static async Task<IResult> GetAllGods(IGodRepository repository, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("Gods");
        logger.LogDebug("GetAllGods called.");
        try
        {
            var gods = await repository.GetAllGodsAsync();
            logger.LogDebug("GetAllGods returned {Count} result(s).", gods.Count);
            return TypedResults.Ok(gods);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetAllGods failed.");
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public static Task<IResult> GetAlllGods(IGodRepository repository, ILoggerFactory loggerFactory) => GetAllGods(repository, loggerFactory);

    public static async Task<IResult> GetGodById(int id, IGodRepository repository, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("Gods");
        if (id <= 0)
        {
            logger.LogWarning("GetGodById called with invalid id {GodId}.", id);
            return TypedResults.BadRequest("Id must be greater than zero.");
        }

        logger.LogDebug("GetGodById called with id {GodId}.", id);
        try
        {
            var god = await repository.GetGodAsync(new GodParameter(id));
            logger.LogDebug("GetGodById succeeded for id {GodId}.", id);
            return TypedResults.Ok(god);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "GetGodById did not find god with id {GodId}.", id);
            return TypedResults.NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetGodById failed for id {GodId}.", id);
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public static async Task<IResult> SearchGodsByName(string name, IGodRepository repository, ILoggerFactory loggerFactory, [FromQuery] bool includeAliases = false)
    {
        var logger = loggerFactory.CreateLogger("Gods");
        if (string.IsNullOrWhiteSpace(name))
        {
            logger.LogWarning("SearchGodsByName called with an empty search term.");
            return TypedResults.BadRequest("Name is required.");
        }

        logger.LogDebug("SearchGodsByName called with Name={Name} IncludeAliases={IncludeAliases}.", name, includeAliases);
        try
        {
            var gods = await repository.GetGodByNameAsync(new GodByNameParameter(name, includeAliases));
            logger.LogDebug("SearchGodsByName returned {Count} result(s) for Name={Name}.", gods.Count, name);
            return TypedResults.Ok(gods);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SearchGodsByName failed for Name={Name}.", name);
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public static async Task<IResult> DeleteAllGods(IGodRepository repository, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("Gods");
        logger.LogWarning("DeleteAllGods called. Destructive operation initiated.");
        try
        {
            await repository.DeleteAllGodsAsync();
            logger.LogInformation("DeleteAllGods completed successfully.");
            return TypedResults.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DeleteAllGods failed.");
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public static async Task<IResult> DeleteGodById(int id, IGodRepository repository, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("Gods");
        if (id <= 0)
        {
            logger.LogWarning("DeleteGodById called with invalid id {GodId}.", id);
            return TypedResults.BadRequest("Id must be greater than zero.");
        }

        logger.LogWarning("DeleteGodById called for id {GodId}.", id);
        try
        {
            await repository.DeleteGodByIdAsync(new GodParameter(id));
            logger.LogInformation("DeleteGodById completed successfully for id {GodId}.", id);
            return TypedResults.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "DeleteGodById did not find god with id {GodId}.", id);
            return TypedResults.NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DeleteGodById failed for id {GodId}.", id);
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
