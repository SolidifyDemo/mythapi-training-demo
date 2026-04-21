using MythApi.Common.Database.Models;
using MythApi.Gods.Models;

namespace MythApi.Gods.Interfaces;

public interface IGodRepository{
    public Task<IList<God>> GetAllGodsAsync(int page = 1, int pageSize = 50);

    public Task<God> GetGodAsync(GodParameter parameter);

    public Task<List<God>> GetGodByNameAsync(GodByNameParameter parameter);

    public Task<List<God>> AddOrUpdateGods(List<GodInput> gods);

    public Task DeleteAllGodsAsync();
}
