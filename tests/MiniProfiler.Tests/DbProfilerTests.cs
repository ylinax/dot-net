﻿using System;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using StackExchange.Profiling.Data;
using Xunit;
using Xunit.Abstractions;

namespace StackExchange.Profiling.Tests
{
    /// <summary>
    /// Tests for <see cref="IDbProfiler"/>.
    /// </summary>
    public class DbProfilerTests : BaseTest, IClassFixture<SqliteFixture>
    {
        public SqliteFixture Fixture;

        public DbProfilerTests(SqliteFixture fixture, ITestOutputHelper output) : base(output)
        {
            Fixture = fixture;
        }

        [Fact]
        public void NonQuery()
        {
            using (var conn = GetConnection())
            {
                var profiler = conn.CountingProfiler;

                conn.Execute("CREATE TABLE TestTable (Id int null)");

                conn.Execute("INSERT INTO TestTable VALUES (1)");
                Assert.Equal(2, profiler.ExecuteStartCount);
                Assert.Equal(2, profiler.ExecuteFinishCount);
                Assert.True(profiler.CompleteStatementMeasured);

                conn.Execute("DELETE FROM TestTable WHERE Id = 1");
                Assert.Equal(3, profiler.ExecuteStartCount);
                Assert.Equal(3, profiler.ExecuteFinishCount);
                Assert.True(profiler.CompleteStatementMeasured);
            }
        }

        [Fact]
        public async Task NonQueryAsync()
        {
            using (var conn = GetConnection())
            {
                var profiler = conn.CountingProfiler;
                conn.Execute("CREATE TABLE TestTable (Id int null)");

                await conn.ExecuteAsync("INSERT INTO TestTable VALUES (1)").ConfigureAwait(false);
                Assert.Equal(2, profiler.ExecuteStartCount);
                Assert.Equal(2, profiler.ExecuteFinishCount);
                Assert.True(profiler.CompleteStatementMeasured);

                await conn.ExecuteAsync("DELETE FROM TestTable WHERE Id = 1").ConfigureAwait(false);
                Assert.Equal(3, profiler.ExecuteStartCount);
                Assert.Equal(3, profiler.ExecuteFinishCount);
                Assert.True(profiler.CompleteStatementMeasured);
            }
        }

        [Fact]
        public void Scalar()
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                var profiler = conn.CountingProfiler;

                cmd.CommandText = "select 1";
                cmd.ExecuteScalar();

                Assert.Equal(1, profiler.ExecuteStartCount);
                Assert.Equal(1, profiler.ExecuteFinishCount);
                Assert.True(profiler.CompleteStatementMeasured);
            }
        }

        [Fact]
        public async Task ScalarAsync()
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                var profiler = conn.CountingProfiler;

                cmd.CommandText = "select 1";
                await cmd.ExecuteScalarAsync().ConfigureAwait(false);

                Assert.Equal(1, profiler.ExecuteStartCount);
                Assert.Equal(1, profiler.ExecuteFinishCount);
                Assert.True(profiler.CompleteStatementMeasured);
            }
        }

        [Fact]
        public void DataReader()
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                var profiler = conn.CountingProfiler;

                cmd.CommandText = "select 1";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.NextResult()) { }
                }

                Assert.Equal(1, profiler.ExecuteStartCount);
                Assert.Equal(1, profiler.ExecuteFinishCount);
#if NETCOREAPP1_1 // .Close() is not exposed in netstandard1.5
                Assert.Equal(0, profiler.ReaderFinishCount);
                Assert.False(profiler.CompleteStatementMeasured);
#else
                Assert.Equal(1, profiler.ReaderFinishCount);
                Assert.True(profiler.CompleteStatementMeasured);
#endif
            }
        }

        [Fact]
        public async Task DataReaderAsync()
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                var profiler = conn.CountingProfiler;

                cmd.CommandText = "select 1";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.NextResultAsync().ConfigureAwait(false)) { }
                }

                Assert.Equal(1, profiler.ExecuteStartCount);
                Assert.Equal(1, profiler.ExecuteFinishCount);
#if NETCOREAPP1_1 // .Close() is not exposed in netstandard1.5
                Assert.Equal(0, profiler.ReaderFinishCount);
                Assert.False(profiler.CompleteStatementMeasured);
#else
                Assert.Equal(1, profiler.ReaderFinishCount);
                Assert.True(profiler.CompleteStatementMeasured);
#endif
            }
        }

        [Fact]
        public void Errors()
        {
            using (var conn = GetConnection())
            {
                const string BadSql = "TROGDOR BURNINATE";

                try
                {
                    conn.Execute(BadSql);
                }
                catch (DbException) { /* yep */ }

                var profiler = conn.CountingProfiler;

                Assert.Equal(1, profiler.ErrorCount);
                Assert.Equal(1, profiler.ExecuteStartCount);
                Assert.Equal(1, profiler.ExecuteFinishCount);
                Assert.Equal(profiler.ErrorSql, BadSql);

                try
                {
                    conn.Query<int>(BadSql);
                }
                catch (DbException) { /* yep */ }

                Assert.Equal(2, profiler.ErrorCount);
                Assert.Equal(2, profiler.ExecuteStartCount);
                Assert.Equal(2, profiler.ExecuteFinishCount);
                Assert.Equal(profiler.ErrorSql, BadSql);

                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = BadSql;
                        cmd.ExecuteScalar();
                    }
                }
                catch (DbException) { /* yep */ }

                Assert.Equal(3, profiler.ExecuteStartCount);
                Assert.Equal(3, profiler.ExecuteFinishCount);
                Assert.Equal(3, profiler.ErrorCount);
                Assert.Equal(profiler.ErrorSql, BadSql);
            }
        }

        [Fact]
        public async Task ErrorsAsync()
        {
            using (var conn = GetConnection())
            {
                const string BadSql = "TROGDOR BURNINATE";

                try
                {
                    await conn.ExecuteAsync(BadSql).ConfigureAwait(false);
                }
                catch (DbException) { /* yep */ }

                var profiler = conn.CountingProfiler;

                Assert.Equal(1, profiler.ErrorCount);
                Assert.Equal(1, profiler.ExecuteStartCount);
                Assert.Equal(1, profiler.ExecuteFinishCount);
                Assert.Equal(profiler.ErrorSql, BadSql);

                try
                {
                    await conn.QueryAsync<int>(BadSql).ConfigureAwait(false);
                }
                catch (DbException) { /* yep */ }

                Assert.Equal(2, profiler.ErrorCount);
                Assert.Equal(2, profiler.ExecuteStartCount);
                Assert.Equal(2, profiler.ExecuteFinishCount);
                Assert.Equal(profiler.ErrorSql, BadSql);

                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = BadSql;
                        await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    }
                }
                catch (DbException) { /* yep */ }

                Assert.Equal(3, profiler.ExecuteStartCount);
                Assert.Equal(3, profiler.ExecuteFinishCount);
                Assert.Equal(3, profiler.ErrorCount);
                Assert.Equal(profiler.ErrorSql, BadSql);
            }
        }

        private CountingConnection GetConnection()
        {
            var connection = Fixture.GetConnection();
            var result = new CountingConnection(connection, new CountingDbProfiler());
            result.Open();
            return result;
        }

        public class CountingConnection : ProfiledDbConnection
        {
            public CountingDbProfiler CountingProfiler { get; set; }

            public CountingConnection(DbConnection connection, IDbProfiler profiler)
                : base(connection, profiler)
            {
                CountingProfiler = (CountingDbProfiler)profiler;
            }
        }
    }

    public class SqliteFixture : IDisposable
    {
        private SqliteConnection Doorstop { get; }
        public SqliteConnection GetConnection() => new SqliteConnection("Data Source= :memory:; Cache = Shared");

        public SqliteFixture()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                Skip.Inconclusive("Sqlite Failure: " + e.Message);
            }
        }

        public void Dispose()
        {
            Doorstop?.Close();
        }
    }
}
