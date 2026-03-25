using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
        private IGodRepository _mockRepository;

        [SetUp]
        public void Setup()
        {
            _mockRepository = Substitute.For<IGodRepository>();
        }

        // --- GetAlllGods ---

        [Test]
        public async Task GetAllGods_ShouldReturnAllGods()
        {
            // Arrange
            var gods = new List<God>
            {
                new God { Name = "Zeus", MythologyId = 1, Description = "God of the sky" },
                new God { Name = "Hera", MythologyId = 1, Description = "Goddess of marriage" }
            };
            _mockRepository.GetAllGodsAsync().Returns(gods);

            // Act
            var result = await Gods.GetAlllGods(_mockRepository);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
        }

        // --- AddOrUpdateGods ---

        [Test]
        public async Task AddOrUpdateGods_ShouldAddNewGod()
        {
            // Arrange
            var godInputs = new List<GodInput>
            {
                new GodInput { Name = "Zeus", MythologyId = 1, Description = "God of the sky" }
            };
            var gods = new List<God>
            {
                new God { Name = "Zeus", MythologyId = 1, Description = "God of the sky" }
            };
            _mockRepository.AddOrUpdateGods(Arg.Any<List<GodInput>>()).Returns(gods);

            // Act
            var result = await Gods.AddOrUpdateGods(godInputs, _mockRepository) as Ok<List<God>>;

            // Assert
            Assert.That(result!.Value!.First().Name, Is.EqualTo("Zeus"));
        }

        [Test]
        public async Task AddOrUpdateGods_EmptyList_ShouldReturnBadRequest()
        {
            // Act
            var result = await Gods.AddOrUpdateGods(new List<GodInput>(), _mockRepository);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequest<string>>());
        }

        [Test]
        public async Task AddOrUpdateGods_ExceedsMaxBatchSize_ShouldReturnBadRequest()
        {
            // Arrange
            var gods = Enumerable.Range(1, 101)
                .Select(i => new GodInput { Name = $"God{i}", MythologyId = 1, Description = $"Description {i}" })
                .ToList();

            // Act
            var result = await Gods.AddOrUpdateGods(gods, _mockRepository);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequest<string>>());
        }

        // --- GetGodById ---

        [Test]
        public async Task GetGodById_ValidId_ShouldReturnOkWithGod()
        {
            // Arrange
            var god = new God { Id = 1, Name = "Zeus", MythologyId = 1, Description = "God of the sky" };
            _mockRepository.GetGodAsync(Arg.Any<GodParameter>()).Returns(god);

            // Act
            var result = await Gods.GetGodById(1, _mockRepository);

            // Assert
            Assert.That(result, Is.InstanceOf<Ok<God>>());
        }

        [Test]
        public async Task GetGodById_ValidId_ShouldReturnCorrectGodName()
        {
            // Arrange
            var god = new God { Id = 1, Name = "Zeus", MythologyId = 1, Description = "God of the sky" };
            _mockRepository.GetGodAsync(Arg.Any<GodParameter>()).Returns(god);

            // Act
            var result = await Gods.GetGodById(1, _mockRepository) as Ok<God>;

            // Assert
            Assert.That(result!.Value!.Name, Is.EqualTo("Zeus"));
        }

        [Test]
        public async Task GetGodById_InvalidId_ShouldReturnBadRequest()
        {
            // Act
            var result = await Gods.GetGodById(0, _mockRepository);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequest<string>>());
        }

        [Test]
        public async Task GetGodById_NonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            _mockRepository.GetGodAsync(Arg.Any<GodParameter>()).Returns((God?)null);

            // Act
            var result = await Gods.GetGodById(999, _mockRepository);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFound>());
        }

        // --- SearchGodsByName ---

        [Test]
        public async Task SearchGodsByName_ValidName_ShouldReturnOkWithResults()
        {
            // Arrange
            var gods = new List<God> { new God { Name = "Zeus", MythologyId = 1, Description = "God of the sky" } };
            _mockRepository.GetGodByNameAsync(Arg.Any<GodByNameParameter>()).Returns(gods);

            // Act
            var result = await Gods.SearchGodsByName("Zeus", _mockRepository);

            // Assert
            Assert.That(result, Is.InstanceOf<Ok<List<God>>>());
        }

        [Test]
        public async Task SearchGodsByName_EmptyName_ShouldReturnBadRequest()
        {
            // Act
            var result = await Gods.SearchGodsByName(" ", _mockRepository);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequest<string>>());
        }

        [Test]
        public async Task SearchGodsByName_IncludeAliasesTrue_ShouldPassFlagToRepository()
        {
            // Arrange
            _mockRepository.GetGodByNameAsync(Arg.Any<GodByNameParameter>()).Returns(new List<God>());

            // Act
            await Gods.SearchGodsByName("Zeus", _mockRepository, includeAliases: true);

            // Assert
            await _mockRepository.Received(1).GetGodByNameAsync(Arg.Is<GodByNameParameter>(p => p.IncludeAliases == true));
        }

        [Test]
        public async Task SearchGodsByName_IncludeAliasesFalse_ShouldPassFlagToRepository()
        {
            // Arrange
            _mockRepository.GetGodByNameAsync(Arg.Any<GodByNameParameter>()).Returns(new List<God>());

            // Act
            await Gods.SearchGodsByName("Zeus", _mockRepository, includeAliases: false);

            // Assert
            await _mockRepository.Received(1).GetGodByNameAsync(Arg.Is<GodByNameParameter>(p => p.IncludeAliases == false));
        }

        // --- DeleteGodById ---

        [Test]
        public async Task DeleteGodById_ValidId_ShouldReturnNoContent()
        {
            // Arrange
            _mockRepository.DeleteGodByIdAsync(Arg.Any<GodParameter>()).Returns(Task.CompletedTask);

            // Act
            var result = await Gods.DeleteGodById(1, _mockRepository);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContent>());
        }

        [Test]
        public async Task DeleteGodById_ValidId_ShouldCallRepositoryWithCorrectId()
        {
            // Arrange
            _mockRepository.DeleteGodByIdAsync(Arg.Any<GodParameter>()).Returns(Task.CompletedTask);

            // Act
            await Gods.DeleteGodById(1, _mockRepository);

            // Assert
            await _mockRepository.Received(1).DeleteGodByIdAsync(Arg.Is<GodParameter>(p => p.Id == 1));
        }

        [Test]
        public async Task DeleteGodById_InvalidId_ShouldReturnBadRequest()
        {
            // Act
            var result = await Gods.DeleteGodById(0, _mockRepository);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequest<string>>());
        }

        [Test]
        public async Task DeleteGodById_NonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            _mockRepository.DeleteGodByIdAsync(Arg.Any<GodParameter>())
                .ThrowsAsync(new InvalidOperationException("God not found."));

            // Act
            var result = await Gods.DeleteGodById(999, _mockRepository);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFound>());
        }
    }
}
