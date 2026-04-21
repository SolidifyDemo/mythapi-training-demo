using MythApi.Common.Database.Models;
using MythApi.Gods.Models;

namespace MythApi.Gods.Interfaces;

public interface IGodRepository{
    public Task<IList<God>> GetAllGodsAsync();

    public Task<God> GetGodAsync(GodParameter parameter);

    public Task<List<God>> GetGodByNameAsync(GodByNameParameter parameter);

    public Task<List<God>> AddOrUpdateGods(List<GodInput> gods);

    /// <summary>
    /// Deletes all gods from the data store.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DeleteAllGodsAsync();

    /// <summary>
    /// Deletes a single god from the data store by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the god to delete.</param>
    /// <returns>
    /// A task representing the asynchronous operation. Returns <c>true</c> if the god was found and deleted;
    /// <c>false</c> if no god with the specified <paramref name="id"/> was found.
    /// </returns>
    public Task<bool> DeleteGodByIdAsync(int id);
}