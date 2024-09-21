using Bunkum.Core;
using Bunkum.Core.Database;
using Bunkum.Core.Services;
using Bunkum.Core.Storage;
using Bunkum.Listener.Request;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Services.OAuth2;
using Refresh.GameServer.Time;

namespace Refresh.GameServer.Types.Data;

public class DataContextService : Service
{
    private readonly StorageService _storageService;
    private readonly MatchService _matchService;
    private readonly AuthenticationService _authService;
    private readonly GuidCheckerService _guidCheckerService;
    private readonly TimeProviderService _timeProviderService;
    private readonly OAuthService _oAuthService;

    public DataContextService(StorageService storage, MatchService match, AuthenticationService auth, Logger logger,
        GuidCheckerService guidChecker, TimeProviderService timeProviderService,
        OAuthService oAuthService) : base(logger)
    {
        this._storageService = storage;
        this._matchService = match;
        this._authService = auth;
        this._guidCheckerService = guidChecker;
        this._timeProviderService = timeProviderService;
        this._oAuthService = oAuthService;
    }

    private static T StealInjection<T>(Service service, ListenerContext? context = null, Lazy<IDatabaseContext>? database = null, string name = "")
    {
        return (T)service.AddParameterToEndpoint(context!, new BunkumParameterInfo(typeof(T), name), database!)!;
    }
    
    public override object? AddParameterToEndpoint(ListenerContext context, BunkumParameterInfo parameter, Lazy<IDatabaseContext> database)
    {
        if (ParameterEqualTo<DataContext>(parameter))
        {
            return new DataContext
            {
                Database = (GameDatabaseContext)database.Value,
                Logger = this.Logger,
                DataStore = StealInjection<IDataStore>(this._storageService),
                Match = this._matchService,
                Token = StealInjection<Token>(this._authService, context, database),
                GuidChecker = this._guidCheckerService,
                TimeProvider = StealInjection<IDateTimeProvider>(this._timeProviderService),
                OAuth = StealInjection<OAuthService>(this._oAuthService),
            };
        }
        
        return null;
    }
}