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
        private IGodRepository _mockRepository;

        [SetUp]
        public void Setup()
        {
            _mockRepository = Substitute.For<IGodRepository>();
        }

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
            var result = await Gods.AddOrUpdateGods(godInputs, _mockRepository);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Zeus"));
        }
    }
}
