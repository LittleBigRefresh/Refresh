using System.Diagnostics;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;
using System.Net;
using System.Reflection;
using Bunkum.HttpServer.Responses;
using Newtonsoft.Json;
using Refresh.GameServer.Types.Matching;

namespace Refresh.GameServer.Services;

public partial class MatchService : EndpointService
{
    private readonly Dictionary<string, IMatchMethod> _matchMethods = new();

    public MatchService(LoggerContainer<BunkumContext> logger) : base(logger)
    {}

    public override void Initialize()
    {
        // TODO: discover match methods via source generation
        List<Type> matchMethodTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IMatchMethod)) && t != typeof(IMatchMethod))
            .ToList();

        foreach (Type type in matchMethodTypes)
        {
            string name = type.Name.Substring(0, type.Name.IndexOf("Method", StringComparison.Ordinal));
            this.Logger.LogTrace(BunkumContext.Service, $"Found {nameof(IMatchMethod)} '{name}'");
            
            this._matchMethods.Add(name, (IMatchMethod)Activator.CreateInstance(type)!);
        }
        
        this.Logger.LogDebug(BunkumContext.Service, $"Discovered {matchMethodTypes.Count} match method types");
    }

    public Response ExecuteMethod(string methodStr, string body)
    {
        if (!this._matchMethods.TryGetValue(methodStr, out IMatchMethod? method))
            return HttpStatusCode.BadRequest;
        
        Debug.Assert(method != null);
        
        JsonSerializer serializer = new();
        using StringReader reader = new(body);
        using JsonTextReader jsonReader = new(reader);

        SerializedRoomData? roomData = serializer.Deserialize<SerializedRoomData>(jsonReader);
        
        // ReSharper disable once InvertIf (happy path goes last)
        if (roomData == null)
        {
            this.Logger.LogWarning(BunkumContext.Matching, "Match data was bad and unserializable, rejecting."); // Already logged data
            return HttpStatusCode.BadRequest;
        }

        return method.Execute(this, this.Logger, roomData);
    }
}