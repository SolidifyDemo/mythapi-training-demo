using MythApi.Common.Database.Models;

namespace MythApi.Regions.Interfaces;

public interface IRegionRepository
{
    public Task<IList<Region>> GetAllRegionsAsync();
}
