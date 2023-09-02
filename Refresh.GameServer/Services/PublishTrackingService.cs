using Bunkum.HttpServer;
using Bunkum.HttpServer.Services;
using MongoDB.Bson;
using NotEnoughLogs;

namespace Refresh.GameServer.Services;

public class PublishTrackingService : EndpointService
{
    public PublishTrackingService(LoggerContainer<BunkumContext> logger) : base(logger) {}

    private readonly HashSet<ObjectId> _usersPublishing = new();

    /// <summary>
    /// Start tracking the user, eg. they started publishing
    /// </summary>
    /// <param name="id"></param>
    public void StartTracking(ObjectId id)
    {
        //Unconditionally add the user to the set
        _ = this._usersPublishing.Add(id);
    }

    /// <summary>
    /// Stop tracking the user, eg. they stopped publishing
    /// </summary>
    /// <param name="id"></param>
    public void StopTracking(ObjectId id)
    {
        //Unconditionally remove the user from the set
        _ = this._usersPublishing.Remove(id);
    }

    public bool IsPublishing(ObjectId id) => this._usersPublishing.Contains(id);
}