using Bunkum.Core.Services;
using MongoDB.Bson;
using NotEnoughLogs;

namespace Refresh.Core.Services;

/// <summary>
/// Documentation on why this exists: <see cref="Endpoints.Game.ChallengeEndpoints.GetContextualScoresForChallenge"/>
/// </summary>
public class ChallengeGhostRateLimitService : EndpointService
{
    public ChallengeGhostRateLimitService(Logger logger, TimeProviderService timeProviderService) : base(logger)
    {
        this._timeProviderService = timeProviderService;
    }

    private readonly TimeProviderService _timeProviderService;

    /// <summary>
    /// User UUIDs -> when their game has last tried to download a ChallengeGhost asset
    /// </summary>
    private readonly Dictionary<ObjectId, DateTimeOffset?> _challengeGhostRateLimitedUsers = new();

    public void AddUserToChallengeGhostRateLimit(ObjectId id)
    {
        // Unconditionally add the user to the set
        this._challengeGhostRateLimitedUsers.Add(id, this._timeProviderService.TimeProvider.Now);
    }

    public void RemoveUserFromChallengeGhostRateLimit(ObjectId id)
    {
        // Unconditionally remove the user from the set
        this._challengeGhostRateLimitedUsers.Remove(id);
    }

    /// <summary>
    /// Whether the user with the given ID should be rate-limited. No rate limit if the user either isn't tracked by this service
    /// or if the last time they've tried downloading a ChallengeGhost asset was over 15 seconds ago, in which case this method
    /// will automatically make this service no longer track the user.
    /// </summary>
    public bool IsUserChallengeGhostRateLimited(ObjectId id)
    {
        KeyValuePair<ObjectId, DateTimeOffset?> usersRateLimit = this._challengeGhostRateLimitedUsers
            .FirstOrDefault(k => k.Key == id);
        
        // No entry
        if (usersRateLimit.Value == null)
            return false;
        
        DateTimeOffset expirationDate = usersRateLimit.Value.Value;

        // Entry has expired, remove it
        if (expirationDate.AddSeconds(15) < this._timeProviderService.TimeProvider.Now)
        {
            this.RemoveUserFromChallengeGhostRateLimit(id);
            return false;
        }

        // Entry has not expired, rate limit should be enforced
        else
        {
            return true;
        }
    }
}