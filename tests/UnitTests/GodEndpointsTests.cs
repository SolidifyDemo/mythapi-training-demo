using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using MythApi.Common.Database.Models;
using MythApi.Endpoints.v1;
using MythApi.Gods.Interfaces;
using MythApi.Gods.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests
{
    public class GodEndpointsTests
    {
        private IGodRepository _repository;

        [SetUp]
        public void Setup()
        {
            _repository = Substitute.For<IGodRepository>();
        }

        [Test]
        public async Task GetAllGods_ShouldReturnAllGods()
        {
            var gods = new List<God>
            {
                new God { Name = "Zeus", MythologyId = 1, Description = "God of the sky" },
                new God { Name = "Hera", MythologyId = 1, Description = "Goddess of marriage" }
            };
            _repository.GetAllGodsAsync().Returns(gods);

            var result = await MythApi.Endpoints.v1.Gods.GetAlllGods(_repository);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task AddOrUpdateGods_ShouldAddNewGod()
        {
            var godInputs = new List<GodInput>
            {
                new GodInput { Name = "Zeus", MythologyId = 1, Description = "God of the sky" }
            };
            var gods = new List<God>
            {
                new God { Name = "Zeus", MythologyId = 1, Description = "God of the sky" }
            };
            _repository.AddOrUpdateGods(Arg.Any<List<GodInput>>()).Returns(gods);

            var result = await Gods.AddOrUpdateGods(godInputs, _repository);

            Assert.That(result, Is.InstanceOf<Ok<List<God>>>());
            var okResult = (Ok<List<God>>)result;
            Assert.That(okResult.Value!.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task AddOrUpdateGods_EmptyList_ShouldReturnBadRequest()
        {
            var result = await Gods.AddOrUpdateGods(new List<GodInput>(), _repository);

            Assert.That(result, Is.InstanceOf<BadRequest<string>>());
        }

        [Test]
        public async Task SearchGodsByName_EmptyName_ShouldReturnBadRequest()
        {
            var result = await Gods.SearchGodsByName(" ", _repository);

            Assert.That(result, Is.InstanceOf<BadRequest<string>>());
        }

        [Test]
        public async Task SearchGodsByName_ValidName_ShouldReturnOk()
        {
            var gods = new List<God>
            {
                new God { Name = "Zeus", MythologyId = 1, Description = "God of the sky" }
            };
            _repository.GetGodByNameAsync(Arg.Any<GodByNameParameter>()).Returns(gods);

            var result = await Gods.SearchGodsByName("Zeus", _repository);

            Assert.That(result, Is.InstanceOf<Ok<List<God>>>());
        }

        [Test]
        public async Task GetGodById_InvalidId_ShouldReturnBadRequest()
        {
            var result = await Gods.GetGodById(0, _repository);

            Assert.That(result, Is.InstanceOf<BadRequest<string>>());
        }

        [Test]
        public async Task GetGodById_NonExistentId_ShouldReturnNotFound()
        {
            _repository.GetGodAsync(Arg.Any<GodParameter>())
                .Returns<God>(_ => throw new InvalidOperationException());

            var result = await Gods.GetGodById(999, _repository);

            Assert.That(result, Is.InstanceOf<NotFound>());
        }

        [Test]
        public async Task DeleteAllGods_ShouldCallRepositoryDeleteAll()
        {
            // Arrange
            _repository.DeleteAllGodsAsync().Returns(Task.CompletedTask);

            // Act
            var result = await Gods.DeleteAllGods(_repository);

            // Assert
            await _repository.Received(1).DeleteAllGodsAsync();
        }

        [Test]
        public async Task DeleteGodById_InvalidId_ShouldReturnBadRequest()
        {
            // Act
            var result = await Gods.DeleteGodById(0, _repository);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequest<string>>());
        }

        [Test]
        public async Task DeleteGodById_NonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            _repository.DeleteGodByIdAsync(Arg.Any<GodParameter>())
                .Returns<Task>(_ => throw new InvalidOperationException());

            // Act
            var result = await Gods.DeleteGodById(999, _repository);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFound>());
        }

        [Test]
        public async Task DeleteGodById_ValidId_ShouldReturnNoContent()
        {
            // Arrange
            _repository.DeleteGodByIdAsync(Arg.Any<GodParameter>()).Returns(Task.CompletedTask);

            // Act
            var result = await Gods.DeleteGodById(1, _repository);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContent>());
            await _repository.Received(1).DeleteGodByIdAsync(Arg.Is<GodParameter>(p => p.Id == 1));
        }
    }
}
