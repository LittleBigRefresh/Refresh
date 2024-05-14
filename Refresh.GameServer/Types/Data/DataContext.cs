using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Data;

public class DataContext
{
    public required GameDatabaseContext Database;
    public required Logger Logger;
    public required IDataStore DataStore;
    public required MatchService Match;
    
    public required Token? Token;
    public GameUser? User => this.Token?.User;
    public TokenGame? Game => this.Token?.TokenGame;
    public TokenPlatform? Platform => this.Token?.TokenPlatform;
}