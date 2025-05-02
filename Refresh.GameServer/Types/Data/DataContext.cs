using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.Database;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Data;

public class DataContext : IDataContext
{
    public required GameDatabaseContext Database;
    public required Logger Logger;
    public required IDataStore DataStore;
    public required MatchService Match;
    public required GuidCheckerService GuidChecker;
    
    public required Token? Token;
    public GameUser? User => this.Token?.User;
    public TokenGame Game => this.Token?.TokenGame ?? TokenGame.Website;
    public TokenPlatform Platform => this.Token?.TokenPlatform ?? TokenPlatform.Website;

    public string GetIconFromHash(string hash)
    {
        return this.Database.GetAssetFromHash(hash)?.GetAsIcon(this.Game, this) ?? hash;
    }
}