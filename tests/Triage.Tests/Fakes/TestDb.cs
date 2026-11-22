using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Triage.Api.Data;

namespace Triage.Tests.Fakes;

/// <summary>Seeded in-memory SQLite. Disposing it drops the database.</summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public AppDb Db { get; }

    public TestDb()
    {
        _connection.Open();
        Db = new AppDb(new DbContextOptionsBuilder<AppDb>().UseSqlite(_connection).Options);
        Db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
