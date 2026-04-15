
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

    public async Task<Mythology?> GetMythologyByGodIdAsync(int godId)
    {
        var god = await _context.Gods.FirstOrDefaultAsync(g => g.Id == godId);
        if (god is null) return null;
        return await _context.Mythologies.FirstOrDefaultAsync(m => m.Id == god.MythologyId);
    }

    public async Task<bool> DeleteMythologyByIdAsync(int id)
    {
        var mythology = await _context.Mythologies.FirstOrDefaultAsync(m => m.Id == id);
        if (mythology is null) return false;
        _context.Mythologies.Remove(mythology);
        await _context.SaveChangesAsync();
        return true;
    }
}
