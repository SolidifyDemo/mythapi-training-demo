using MythApi.Common.Database.Models;
using MythApi.Mythologies.Interfaces;
using Microsoft.Extensions.Logging;

public static class Mythologies
{
    public static void RegisterMythologiesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var mythologies = endpoints.MapGroup("/api/v1/mythologies");

        mythologies.MapGet("", GetAllMythologies);
    }

    public static async Task<IResult> GetAllMythologies(IMythologyRepository repository, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("Mythologies");
        logger.LogDebug("GetAllMythologies called.");
        try
        {
            var mythologies = await repository.GetAllMythologiesAsync();
            logger.LogDebug("GetAllMythologies returned {Count} result(s).", mythologies.Count);
            return TypedResults.Ok(mythologies);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetAllMythologies failed.");
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
