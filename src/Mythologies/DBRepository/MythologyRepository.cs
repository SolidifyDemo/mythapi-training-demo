
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

    public async Task<Mythology?> GetMythologyByGodIdAsync(int godId)
    {
        return await (
            from god in _context.Gods
            where god.Id == godId
            join mythology in _context.Mythologies on god.MythologyId equals mythology.Id
            select mythology
        ).FirstOrDefaultAsync();
    }
}
