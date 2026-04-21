using MythApi.Common.Database.Models;
using MythApi.Regions.Interfaces;

public static class Regions
{
    public static void RegisterRegionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var regions = endpoints.MapGroup("/api/v1/regions");

        regions.MapGet("", GetAllRegions);
    }

    public static Task<IList<Region>> GetAllRegions(IRegionRepository repository) => repository.GetAllRegionsAsync();
}
