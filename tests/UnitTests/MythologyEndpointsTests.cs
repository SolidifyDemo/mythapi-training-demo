using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using MythApi.Common.Database.Models;
using MythApi.Mythologies.Interfaces;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTests
{
    public class MythologyEndpointsTests
    {
        private IMythologyRepository _mockRepository;

        [SetUp]
        public void Setup()
        {
            _mockRepository = Substitute.For<IMythologyRepository>();
        }

        // --- GetAllMythologies ---

        [Test]
        public async Task GetAllMythologies_WhenRepositoryReturnsEmpty_ShouldReturnEmptyList()
        {
            // Arrange
            _mockRepository.GetAllMythologiesAsync().Returns(new List<Mythology>());

            // Act
            var result = await Mythologies.GetAllMythologies(_mockRepository);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllMythologies_WhenRepositoryReturnsSingle_ShouldReturnOneMythology()
        {
            // Arrange
            var expected = new List<Mythology> { new Mythology { Id = 1, Name = "Norse" } };
            _mockRepository.GetAllMythologiesAsync().Returns(expected);

            // Act
            var result = await Mythologies.GetAllMythologies(_mockRepository);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task GetAllMythologies_WhenRepositoryReturnsMultiple_ShouldReturnAll()
        {
            // Arrange
            var expected = new List<Mythology>
            {
                new Mythology { Id = 1, Name = "Norse" },
                new Mythology { Id = 2, Name = "Greek" },
                new Mythology { Id = 3, Name = "Roman" }
            };
            _mockRepository.GetAllMythologiesAsync().Returns(expected);

            // Act
            var result = await Mythologies.GetAllMythologies(_mockRepository);

            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
        }

        // --- GetMythologyById ---

        [Test]
        public async Task GetMythologyById_WhenMythologyExists_ShouldReturnOkWithMythology()
        {
            // Arrange
            var mythology = new Mythology { Id = 1, Name = "Norse" };
            _mockRepository.GetMythologyByIdAsync(1).Returns(mythology);

            // Act
            var result = await Mythologies.GetMythologyById(1, _mockRepository);

            // Assert
            Assert.That(result, Is.InstanceOf<Ok<Mythology>>());
        }

        [Test]
        public async Task GetMythologyById_WhenMythologyExists_ShouldReturnCorrectMythologyName()
        {
            // Arrange
            var mythology = new Mythology { Id = 2, Name = "Greek" };
            _mockRepository.GetMythologyByIdAsync(2).Returns(mythology);

            // Act
            var result = await Mythologies.GetMythologyById(2, _mockRepository) as Ok<Mythology>;

            // Assert
            Assert.That(result!.Value!.Name, Is.EqualTo("Greek"));
        }

        [Test]
        public async Task GetMythologyById_WhenMythologyDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            _mockRepository.GetMythologyByIdAsync(99).Returns((Mythology?)null);

            // Act
            var result = await Mythologies.GetMythologyById(99, _mockRepository);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFound>());
        }

        [Test]
        public async Task GetMythologyById_WhenCalled_ShouldPassCorrectIdToRepository()
        {
            // Arrange
            var mythology = new Mythology { Id = 5, Name = "Egyptian" };
            _mockRepository.GetMythologyByIdAsync(5).Returns(mythology);

            // Act
            await Mythologies.GetMythologyById(5, _mockRepository);

            // Assert
            await _mockRepository.Received(1).GetMythologyByIdAsync(5);
        }

        [Test]
        public void GetMythologyById_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            _mockRepository.GetMythologyByIdAsync(Arg.Any<int>()).ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(() => Mythologies.GetMythologyById(1, _mockRepository));
        }
    }
}
