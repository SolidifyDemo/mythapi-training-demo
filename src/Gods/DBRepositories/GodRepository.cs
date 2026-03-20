using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MythApi.Gods.Interfaces;
using MythApi.Common.Database.Models;
using MythApi.Common.Database;
using MythApi.Gods.Models;

namespace MythApi.Gods.DBRepositories;

public class GodRepository : IGodRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<GodRepository> _logger;

    public GodRepository(AppDbContext context, ILogger<GodRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<God>> AddOrUpdateGods(List<GodInput> gods)
    {
        _logger.LogInformation("Adding or updating {GodCount} gods", gods.Count);
        try
        {
            foreach(var god in gods) {
                if (god.Id.HasValue && _context.Gods.Any(x => x.Id == god.Id))
                {
                    _context.Gods.Where(x => x.Id == god.Id)
                        .ExecuteUpdate(setter => 
                            setter.SetProperty(x => x.Name, god.Name)
                                .SetProperty(x => x.Description, god.Description)
                            );
                }
                else
                {
                    var newGod = new God
                    {
                        Name = god.Name,
                        MythologyId = god.MythologyId,
                        Description = god.Description
                    };
                    _context.Gods.Add(newGod);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully saved {GodCount} gods", gods.Count);
            return await _context.Gods.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {MethodName}", nameof(AddOrUpdateGods));
            throw;
        }
    }

    public async Task<IList<God>> GetAllGodsAsync()
    {
        _logger.LogInformation("Fetching all gods from database");
        try
        {
            var gods = await _context.Gods.ToListAsync();
            foreach (var god in gods)
            {
                _context.Entry(god).Collection(x => x.Aliases).Load();
            }
            _logger.LogInformation("Retrieved {GodCount} gods", gods.Count);
            return gods;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {MethodName}", nameof(GetAllGodsAsync));
            throw;
        }
    }

    public async Task<God> GetGodAsync(GodParameter parameter)
    {
        _logger.LogInformation("Fetching god with id {GodId}", parameter.Id);
        try
        {
            return await _context.Gods.FirstAsync(x => x.Id == parameter.Id);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "God with id {GodId} not found", parameter.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {MethodName}", nameof(GetGodAsync));
            throw;
        }
    }

    public async Task<List<God>> GetGodByNameAsync(GodByNameParameter parameter)
    {
        _logger.LogInformation("Searching gods by name {Name}, includeAliases={IncludeAliases}", parameter.Name, parameter.IncludeAliases);
        try
        {
            var namePattern = $"%{parameter.Name}%";

            List<God> result;
            if (parameter.IncludeAliases)
            {
                result = await _context.Gods
                    .Where(g => EF.Functions.Like(g.Name, namePattern) ||
                                g.Aliases.Any(a => EF.Functions.Like(a.Name, namePattern)))
                    .ToListAsync();
            }
            else
            {
                result = await _context.Gods
                    .Where(g => EF.Functions.Like(g.Name, namePattern))
                    .ToListAsync();
            }

            _logger.LogInformation("Found {GodCount} gods matching name {Name}", result.Count, parameter.Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {MethodName}", nameof(GetGodByNameAsync));
            throw;
        }
    }
}