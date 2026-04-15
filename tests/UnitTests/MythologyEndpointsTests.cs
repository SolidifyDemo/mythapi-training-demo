using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using MythApi.Common.Database.Models;
using MythApi.Mythologies.Interfaces;
using NUnit.Framework;
using System.Threading.Tasks;

namespace UnitTests
{
    public class MythologyEndpointsTests
    {
        private IMythologyRepository _repository;

        [SetUp]
        public void Setup()
        {
            _repository = Substitute.For<IMythologyRepository>();
        }

        [Test]
        public async Task GetMythologyByGod_WhenGodExists_ShouldReturnOkWithMythology()
        {
            // Arrange
            var mythology = new Mythology { Id = 1, Name = "Norse" };
            _repository.GetMythologyByGodIdAsync(1).Returns(mythology);

            // Act
            var result = await Mythologies.GetMythologyByGod(1, _repository);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<Ok<Mythology>>());
        }

        [Test]
        public async Task GetMythologyByGod_WhenGodExists_ShouldReturnCorrectMythologyName()
        {
            // Arrange
            var mythology = new Mythology { Id = 1, Name = "Norse" };
            _repository.GetMythologyByGodIdAsync(1).Returns(mythology);

            // Act
            var result = await Mythologies.GetMythologyByGod(1, _repository);

            // Assert
            var okResult = (Ok<Mythology>)result.Result;
            Assert.That(okResult.Value!.Name, Is.EqualTo("Norse"));
        }

        [Test]
        public async Task GetMythologyByGod_WhenGodDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            _repository.GetMythologyByGodIdAsync(999).Returns((Mythology?)null);

            // Act
            var result = await Mythologies.GetMythologyByGod(999, _repository);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFound>());
        }

        [Test]
        public async Task GetMythologyByGod_WhenCalled_ShouldPassCorrectGodIdToRepository()
        {
            // Arrange
            var mythology = new Mythology { Id = 2, Name = "Greek" };
            _repository.GetMythologyByGodIdAsync(42).Returns(mythology);

            // Act
            await Mythologies.GetMythologyByGod(42, _repository);

            // Assert
            await _repository.Received(1).GetMythologyByGodIdAsync(42);
        }

        [Test]
        public async Task GetMythologyByGod_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            _repository.GetMythologyByGodIdAsync(Arg.Any<int>())
                .Returns<Mythology?>(_ => throw new InvalidOperationException("Repository error"));

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await Mythologies.GetMythologyByGod(1, _repository));
        }
    }
}
