using Bunkum.EntityFrameworkDatabase;
using Microsoft.EntityFrameworkCore;

namespace Refresh.GameServer.Database.Postgres;

public class PostgresGameDatabaseProvider : EntityFrameworkDatabaseProvider<PostgresGameDatabaseContext>
{
    public PostgresGameDatabaseProvider(Action<DbContextOptionsBuilder> configureAction) : base(configureAction)
    {}
    
    
}