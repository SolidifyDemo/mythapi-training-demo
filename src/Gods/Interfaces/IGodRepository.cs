using MythApi.Common.Database.Models;
using MythApi.Gods.Models;

namespace MythApi.Gods.Interfaces;

public interface IGodRepository{
    public Task<IList<God>> GetAllGodsAsync();

    public Task<God?> GetGodAsync(GodParameter parameter);

    public Task<List<God>> GetGodByNameAsync(GodByNameParameter parameter);

    public Task<List<God>> AddOrUpdateGods(List<GodInput> gods);

    /// <summary>
    /// Deletes a single god by its unique identifier.
    /// </summary>
    /// <param name="parameter">The parameter containing the ID of the god to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no god with the given ID exists.</exception>
    public Task DeleteGodByIdAsync(GodParameter parameter);
}