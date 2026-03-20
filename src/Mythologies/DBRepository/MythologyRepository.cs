
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MythApi.Common.Database;
using MythApi.Common.Database.Models;
using MythApi.Mythologies.Interfaces;

namespace MythApi.Mythologies.DBRepositories;

public class MythologyRepository : IMythologyRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<MythologyRepository> _logger;

    public MythologyRepository(AppDbContext context, ILogger<MythologyRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IList<Mythology>> GetAllMythologiesAsync()
    {
        _logger.LogInformation("Fetching all mythologies from database");
        try
        {
            var results = await _context.Mythologies.ToListAsync();
            _logger.LogInformation("Retrieved {MythologyCount} mythologies", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching mythologies");
            throw;
        }
    }
}
