
using Microsoft.EntityFrameworkCore;
using MythApi.Common.Database;
using MythApi.Common.Database.Models;
using MythApi.Mythologies.Interfaces;

namespace MythApi.Mythologies.DBRepositories;

public class MythologyRepository : IMythologyRepository
{
    private readonly AppDbContext _context;

    public MythologyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IList<Mythology>> GetAllMythologiesAsync()
    {
        return await _context.Mythologies.ToListAsync();
    }

    public async Task<Mythology?> GetMythologyByIdAsync(int id)
    {
        return await _context.Mythologies.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Mythology?> GetMythologyByGodNameAsync(string godName)
    {
        var god = await _context.Gods
            .FirstOrDefaultAsync(g => g.Name.ToLower() == godName.ToLower());
        
        if (god is null)
            return null;

        return await _context.Mythologies
            .Include(m => m.Gods)
            .FirstOrDefaultAsync(m => m.Id == god.MythologyId);
    }
}
