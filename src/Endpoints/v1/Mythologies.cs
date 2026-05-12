using Microsoft.Extensions.Logging;
using MythApi.Common.Database.Models;
using MythApi.Mythologies.Interfaces;

public static class Mythologies
{
    public static void RegisterMythologiesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var mythologies = endpoints.MapGroup("/api/v1/mythologies");

        mythologies.MapGet("", GetAllMythologies);
    }

    public static async Task<IResult> GetAllMythologies(IMythologyRepository repository, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(nameof(Mythologies));
        try
        {
            logger.LogInformation("Processing request for all mythologies.");
            var result = await repository.GetAllMythologiesAsync();
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching mythologies.");
            return Results.Problem("Failed to retrieve mythologies. Please contact support if the problem persists.", statusCode: 500);
        }
    }
}
