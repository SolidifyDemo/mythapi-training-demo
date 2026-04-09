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
        var searchPattern = $"%{parameter.Name}%";

        IQueryable<God> query = _context.Gods
            .Where(g => EF.Functions.Like(g.Name, searchPattern));

        if (parameter.IncludeAliases)
        {
            var godIdsByAlias = _context.Aliases
                .Where(a => EF.Functions.Like(a.Name, searchPattern))
                .Select(a => a.GodId);

            query = query.Union(
                _context.Gods.Where(g => godIdsByAlias.Contains(g.Id)));
        }

        return await query.ToListAsync();
    }

    public async Task DeleteAllGodsAsync()
    {
        await _context.Gods.ExecuteDeleteAsync();
    }

    public async Task DeleteGodByIdAsync(GodParameter parameter)
    {
        var deleted = await _context.Gods
            .Where(g => g.Id == parameter.Id)
            .ExecuteDeleteAsync();

        if (deleted == 0)
            throw new InvalidOperationException($"God with id {parameter.Id} not found.");
    }
}