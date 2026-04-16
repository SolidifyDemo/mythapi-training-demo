using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using MythApi.Common.Database.Models;
using MythApi.Endpoints.v1;
using MythApi.Gods.Interfaces;
using MythApi.Gods.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests
{
    public class GodEndpointsTests
    {
        private IGodRepository _repository;
        private TestLogger _logger;
        private ILoggerFactory _loggerFactory;

        [SetUp]
        public void Setup()
        {
            _repository = Substitute.For<IGodRepository>();
            _logger = new TestLogger();
            _loggerFactory = new TestLoggerFactory(_logger);
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

            var result = await Gods.GetAllGods(_repository, _loggerFactory);

            Assert.That(result, Is.InstanceOf<Ok<IList<God>>>());
            Assert.That(HasLog(LogLevel.Debug, "GetAllGods called"), Is.True);
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

            var result = await Gods.AddOrUpdateGods(godInputs, _repository, _loggerFactory);

            Assert.That(result, Is.InstanceOf<Ok<List<God>>>());
            var okResult = (Ok<List<God>>)result;
            Assert.That(okResult.Value!.Count, Is.EqualTo(1));
            Assert.That(HasLog(LogLevel.Information, "AddOrUpdateGods called"), Is.True);
            Assert.That(HasLog(LogLevel.Information, "completed successfully"), Is.True);
        }

        [Test]
        public async Task AddOrUpdateGods_EmptyList_ShouldReturnBadRequest()
        {
            var result = await Gods.AddOrUpdateGods(new List<GodInput>(), _repository, _loggerFactory);

            Assert.That(result, Is.InstanceOf<BadRequest<string>>());
            Assert.That(HasLog(LogLevel.Warning, "empty payload"), Is.True);
        }

        [Test]
        public async Task SearchGodsByName_EmptyName_ShouldReturnBadRequest()
        {
            var result = await Gods.SearchGodsByName(" ", _repository, _loggerFactory);

            Assert.That(result, Is.InstanceOf<BadRequest<string>>());
            Assert.That(HasLog(LogLevel.Warning, "empty search term"), Is.True);
        }

        [Test]
        public async Task SearchGodsByName_ValidName_ShouldReturnOk()
        {
            var gods = new List<God>
            {
                new God { Name = "Zeus", MythologyId = 1, Description = "God of the sky" }
            };
            _repository.GetGodByNameAsync(Arg.Any<GodByNameParameter>()).Returns(gods);

            var result = await Gods.SearchGodsByName("Zeus", _repository, _loggerFactory);

            Assert.That(result, Is.InstanceOf<Ok<List<God>>>());
            Assert.That(HasLog(LogLevel.Debug, "SearchGodsByName called"), Is.True);
        }

        [Test]
        public async Task GetGodById_InvalidId_ShouldReturnBadRequest()
        {
            var result = await Gods.GetGodById(0, _repository, _loggerFactory);

            Assert.That(result, Is.InstanceOf<BadRequest<string>>());
            Assert.That(HasLog(LogLevel.Warning, "invalid id"), Is.True);
        }

        [Test]
        public async Task GetGodById_NonExistentId_ShouldReturnNotFound()
        {
            _repository.GetGodAsync(Arg.Any<GodParameter>())
                .Returns<God>(_ => throw new InvalidOperationException());

            var result = await Gods.GetGodById(999, _repository, _loggerFactory);

            Assert.That(result, Is.InstanceOf<NotFound>());
            Assert.That(HasLog(LogLevel.Warning, "did not find"), Is.True);
        }

        [Test]
        public async Task DeleteAllGods_ShouldCallRepositoryDeleteAll()
        {
            // Arrange
            _repository.DeleteAllGodsAsync().Returns(Task.CompletedTask);

            // Act
            var result = await Gods.DeleteAllGods(_repository, _loggerFactory);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContent>());
            await _repository.Received(1).DeleteAllGodsAsync();
            Assert.That(HasLog(LogLevel.Warning, "Destructive operation"), Is.True);
            Assert.That(HasLog(LogLevel.Information, "completed successfully"), Is.True);
        }

        [Test]
        public async Task DeleteGodById_InvalidId_ShouldReturnBadRequest()
        {
            // Act
            var result = await Gods.DeleteGodById(0, _repository, _loggerFactory);

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
            var result = await Gods.DeleteGodById(999, _repository, _loggerFactory);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFound>());
        }

        [Test]
        public async Task DeleteGodById_ValidId_ShouldReturnNoContent()
        {
            // Arrange
            _repository.DeleteGodByIdAsync(Arg.Any<GodParameter>()).Returns(Task.CompletedTask);

            // Act
            var result = await Gods.DeleteGodById(1, _repository, _loggerFactory);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContent>());
            await _repository.Received(1).DeleteGodByIdAsync(Arg.Is<GodParameter>(p => p.Id == 1));
        }

        [Test]
        public async Task AddOrUpdateGods_WhenRepositoryThrows_ShouldReturn500AndLogError()
        {
            var godInputs = new List<GodInput>
            {
                new GodInput { Name = "Zeus", MythologyId = 1, Description = "God of the sky" }
            };
            _repository.AddOrUpdateGods(Arg.Any<List<GodInput>>())
                .Returns<Task<List<God>>>(_ => throw new Exception("boom"));

            var result = await Gods.AddOrUpdateGods(godInputs, _repository, _loggerFactory);

            Assert.That(result, Is.InstanceOf<StatusCodeHttpResult>());
            Assert.That(((StatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(HasLog(LogLevel.Error, "AddOrUpdateGods failed"), Is.True);
        }

        [Test]
        public async Task DeleteAllGods_WhenRepositoryThrows_ShouldReturn500AndLogError()
        {
            _repository.DeleteAllGodsAsync()
                .Returns<Task>(_ => throw new Exception("boom"));

            var result = await Gods.DeleteAllGods(_repository, _loggerFactory);

            Assert.That(result, Is.InstanceOf<StatusCodeHttpResult>());
            Assert.That(((StatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(HasLog(LogLevel.Error, "DeleteAllGods failed"), Is.True);
        }

        private bool HasLog(LogLevel level, string messageFragment)
        {
            return _logger.LogEntries.Any(entry =>
                entry.Level == level &&
                entry.Message.Contains(messageFragment, StringComparison.OrdinalIgnoreCase));
        }

        private sealed class TestLoggerFactory(TestLogger logger) : ILoggerFactory
        {
            public void AddProvider(ILoggerProvider provider)
            {
            }

            public ILogger CreateLogger(string categoryName) => logger;

            public void Dispose()
            {
            }
        }

        private sealed class TestLogger : ILogger
        {
            public IList<LogEntry> LogEntries { get; } = new List<LogEntry>();

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                LogEntries.Add(new LogEntry(logLevel, formatter(state, exception)));
            }
        }

        private sealed record LogEntry(LogLevel Level, string Message);

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
