using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.GameServer.Services;
using Refresh.Database.Models.Users;

namespace Refresh.GameServer.Types.Data;

public class DataContext : IDataContext
{
    public required GameDatabaseContext Database { get; init; }
    public required Logger Logger { get; init; }
    public required IDataStore DataStore { get; init; }
    public required MatchService Match { get; init; }
    public required GuidCheckerService GuidChecker { get; init; }
    
    public required Token? Token;
    public GameUser? User => this.Token?.User;
    public TokenGame Game => this.Token?.TokenGame ?? TokenGame.Website;
    public TokenPlatform Platform => this.Token?.TokenPlatform ?? TokenPlatform.Website;

    public string GetIconFromHash(string hash)
    {
        return this.Database.GetAssetFromHash(hash)?.GetAsIcon(this.Game, this) ?? hash;
    }
}