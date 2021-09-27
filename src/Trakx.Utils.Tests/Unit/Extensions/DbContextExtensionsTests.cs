using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Serilog;
using System;
using System.Collections.Generic;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Extensions
{
    public class DbContextExtensionsTests : IDisposable
    {
        private readonly DbContext _inMemoryDbContext;
        private readonly SqliteConnection _dbConnection;

        public DbContextExtensionsTests()
        {
            _dbConnection = new SqliteConnection("Filename=:memory:");
            var options = new DbContextOptionsBuilder<DbContext>()
               .UseSqlite(_dbConnection)
               .Options;
            _inMemoryDbContext = new DbContext(options);
        }

        [Fact]
        public void DbContextExtensions_should_log_migrations()
        {
            var fakeLogger = Substitute.For<ILogger>();
            _inMemoryDbContext.LogMigrations(fakeLogger);
            fakeLogger.Received().Information(Arg.Any<string>(), Arg.Any<List<string>>());
        }

        public void Dispose()
        {
            _dbConnection.Dispose();
            _inMemoryDbContext.Dispose();
        }
    }
}
