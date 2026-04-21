using Microsoft.EntityFrameworkCore;
using MythApi.Common.Database;
using MythApi.Common.Database.Models;
using MythApi.Regions.Interfaces;

namespace MythApi.Regions.DBRepositories;

public class RegionRepository : IRegionRepository
{
    private readonly AppDbContext _context;

    public RegionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IList<Region>> GetAllRegionsAsync()
    {
        return await _context.Regions.ToListAsync();
    }
}
