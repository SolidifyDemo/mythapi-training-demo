using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MythApi.Common.Database.Models;
using MythApi.Endpoints.v1;
using MythApi.Gods.Interfaces;
using MythApi.Gods.Models;
using NUnit.Framework;

namespace IntegrationTests;

public class GodsErrorHandlingTests
{
    [Test]
    public async Task AddOrUpdateGods_WhenRepositoryThrows_ShouldReturnProblemResult()
    {
        var repository = new ThrowingGodRepository(throwOnAddOrUpdate: true);

        var result = await Gods.AddOrUpdateGods(new List<GodInput>(), repository, NullLoggerFactory.Instance);

        Assert.That(result, Is.AssignableTo<IStatusCodeHttpResult>());
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task DeleteAllGods_WhenRepositoryThrows_ShouldReturnProblemResult()
    {
        var repository = new ThrowingGodRepository(throwOnDeleteAll: true);

        var result = await Gods.DeleteAllGods(repository, NullLoggerFactory.Instance);

        Assert.That(result, Is.AssignableTo<IStatusCodeHttpResult>());
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    private sealed class ThrowingGodRepository : IGodRepository
    {
        private readonly bool _throwOnAddOrUpdate;
        private readonly bool _throwOnDeleteAll;

        public ThrowingGodRepository(bool throwOnAddOrUpdate = false, bool throwOnDeleteAll = false)
        {
            _throwOnAddOrUpdate = throwOnAddOrUpdate;
            _throwOnDeleteAll = throwOnDeleteAll;
        }

        public Task<IList<God>> GetAllGodsAsync() => Task.FromResult((IList<God>)new List<God>());

        public Task<God> GetGodAsync(GodParameter parameter) => Task.FromResult(new God());

        public Task<List<God>> GetGodByNameAsync(GodByNameParameter parameter) => Task.FromResult(new List<God>());

        public Task<List<God>> AddOrUpdateGods(List<GodInput> gods)
        {
            if (_throwOnAddOrUpdate)
            {
                throw new InvalidOperationException("boom");
            }

            return Task.FromResult(new List<God>());
        }

        public Task DeleteAllGodsAsync()
        {
            if (_throwOnDeleteAll)
            {
                throw new InvalidOperationException("boom");
            }

            return Task.CompletedTask;
        }
    }
}
