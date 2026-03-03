using Microsoft.EntityFrameworkCore;
using MythApi.Gods.Interfaces;
using MythApi.Common.Database.Models;
using MythApi.Common.Database;
using MythApi.Gods.Models;

namespace MythApi.Gods.DBRepositories;

public class GodRepository : IGodRepository
{
    private readonly AppDbContext _context;

    public GodRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<God>> AddOrUpdateGods(List<GodInput> gods)
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
        return await _context.Gods.ToListAsync();
    }

    public async Task<IList<God>> GetAllGodsAsync()
    {
        var gods = await _context.Gods.ToListAsync();
        foreach (var god in gods)
        {
            _context.Entry(god).Collection(x => x.Aliases).Load();
        }
        return gods;
    }

    public async Task<God> GetGodAsync(GodParameter parameter)
    {
        return await _context.Gods.FirstAsync(x => x.Id == parameter.Id);
    }

    public async Task<List<God>> GetGodByNameAsync(GodByNameParameter parameter)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(parameter.Name))
        {
            return new List<God>();
        }

        IQueryable<God> query;
        
        if (parameter.IncludeAliases)
        {
            // Search in both God names and Alias names using LINQ
            query = _context.Gods
                .Where(g => g.Name.Contains(parameter.Name) || 
                           g.Aliases.Any(a => a.Name.Contains(parameter.Name)));
        }
        else
        {
            // Search only in God names
            query = _context.Gods
                .Where(g => g.Name.Contains(parameter.Name));
        }

        return await query.ToListAsync();
    }
}