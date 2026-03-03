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
        // Validation: Check if the list is empty
        if (gods == null || gods.Count == 0)
        {
            throw new ArgumentException("The list of gods cannot be empty.");
        }

        // Validation: Limit the size of bulk operations to prevent resource exhaustion
        const int maxBulkSize = 100;
        if (gods.Count > maxBulkSize)
        {
            throw new ArgumentException($"Bulk operation size cannot exceed {maxBulkSize} items. Received {gods.Count} items.");
        }

        // Validation: Validate each god's required fields before processing
        var validationErrors = new List<string>();
        for (int i = 0; i < gods.Count; i++)
        {
            var god = gods[i];
            if (string.IsNullOrWhiteSpace(god.Name))
            {
                validationErrors.Add($"Item {i}: Name is required.");
            }
            if (string.IsNullOrWhiteSpace(god.Description))
            {
                validationErrors.Add($"Item {i}: Description is required.");
            }
            if (god.MythologyId <= 0)
            {
                validationErrors.Add($"Item {i}: Valid MythologyId is required.");
            }
        }

        if (validationErrors.Any())
        {
            throw new ArgumentException($"Validation failed: {string.Join("; ", validationErrors)}");
        }

        // Validation: Check that all MythologyIds exist in the database
        var mythologyIds = gods.Select(g => g.MythologyId).Distinct().ToList();
        var existingMythologyIds = await _context.Mythologies
            .Where(m => mythologyIds.Contains(m.Id))
            .Select(m => m.Id)
            .ToListAsync();

        var invalidMythologyIds = mythologyIds.Except(existingMythologyIds).ToList();
        if (invalidMythologyIds.Any())
        {
            throw new ArgumentException($"Invalid MythologyId(s): {string.Join(", ", invalidMythologyIds)}. These mythologies do not exist in the database.");
        }

        // Use a database transaction to ensure atomicity (all or nothing)
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Load existing god IDs to avoid N+1 query problem
            var inputGodIds = gods.Where(g => g.Id.HasValue).Select(g => g.Id!.Value).ToList();
            var existingGodIds = await _context.Gods
                .Where(x => inputGodIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();
            
            var affectedGodIds = new List<int>();

            foreach(var god in gods) {
                if (god.Id.HasValue && existingGodIds.Contains(god.Id.Value))
                {
                    _context.Gods.Where(x => x.Id == god.Id)
                        .ExecuteUpdate(setter => 
                            setter.SetProperty(x => x.Name, god.Name)
                                .SetProperty(x => x.Description, god.Description)
                                .SetProperty(x => x.MythologyId, god.MythologyId)
                            );
                    affectedGodIds.Add(god.Id.Value);
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
                    // Note: newGod.Id will be set after SaveChangesAsync
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            // Return only the affected gods for efficiency
            // For new gods, get them by matching name and mythologyId since we don't have IDs before save
            var newGodNames = gods.Where(g => !g.Id.HasValue).Select(g => g.Name).ToList();
            var result = await _context.Gods
                .Where(g => affectedGodIds.Contains(g.Id) || newGodNames.Contains(g.Name))
                .ToListAsync();
            
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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

    public Task<List<God>> GetGodByNameAsync(GodByNameParameter parameter)
    {
        var query = parameter.IncludeAliases ? $"SELECT * FROM God WHERE Name LIKE '%{parameter.Name}%' or Id in (SELECT GodId FROM Alias WHERE Name LIKE '%{parameter.Name}%')" : $"SELECT * FROM God WHERE Name LIKE '%{parameter.Name}%'";
        var result = _context.Gods.FromSqlRaw(query).ToList();

        return Task.FromResult(result);
    }
}