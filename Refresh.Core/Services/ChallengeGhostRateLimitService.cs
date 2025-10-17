using Bunkum.Core.Services;
using MongoDB.Bson;
using NotEnoughLogs;

namespace Refresh.Core.Services;

/// <summary>
/// This service is part of a workaround for a LBP Hub player challenge bug, where the game will break a challenge score's ghost replay
/// (if it's not the user's own score) by first sending a request to /challenge/{challengeId}/scoreboard/{username}/contextual
/// to find out the next best score, then downloading that score's ChallengeGhost asset as intended, but then downloading the asset
/// of every score in the contextual leaderboard response (so first the intended one, then the intended one again and a few others).
/// The game will then seemingly combine these assets' ghost frames into one replay, vastly increasing their coordinates and breaking
/// the replay that way. 
/// Since the game only ever requests multiple ChallengeGhost assets in a row when it is about to do this bug, we can simply use
/// this service to temporarily block all ghost asset requests after the first, correct one. This rate limit should also be cancelled if
/// the game sends any challenge score requests, since it doesn't send them while requesting the assets, so we can be sure the game
/// is not about to break replay at that point.
/// </summary>
public class ChallengeGhostRateLimitService : EndpointService
{
    public ChallengeGhostRateLimitService(Logger logger, TimeProviderService timeProviderService) : base(logger)
    {
        this._timeProviderService = timeProviderService;
    }

    private readonly TimeProviderService _timeProviderService;

    /// <summary>
    /// User UUID -> when the rate-limit has started (first successful ChallengeGhost download)
    /// </summary>
    private readonly Dictionary<ObjectId, DateTimeOffset?> _rateLimitedUsers = new();

    public void AddUserToRateLimit(ObjectId id)
    {
        // Unconditionally add the user to the set
        this._rateLimitedUsers.Add(id, this._timeProviderService.TimeProvider.Now);
    }

    public void RemoveUserFromRateLimit(ObjectId id)
    {
        // Unconditionally remove the user from the set
        this._rateLimitedUsers.Remove(id);
    }

    /// <summary>
    /// Whether the user with the given ID should be rate-limited. No rate limit if the user either isn't tracked by this service
    /// or if the last time they've tried downloading a ChallengeGhost asset was over 15 seconds ago, in which case this method
    /// will automatically make this service no longer track the user.
    /// </summary>
    public bool IsUserRateLimited(ObjectId id)
    {
        KeyValuePair<ObjectId, DateTimeOffset?> usersRateLimit = this._rateLimitedUsers
            .FirstOrDefault(k => k.Key == id);
        
        // No entry
        if (usersRateLimit.Value == null)
            return false;
        
        DateTimeOffset expirationDate = usersRateLimit.Value.Value;

        // Entry has expired, remove it
        if (expirationDate.AddSeconds(15) < this._timeProviderService.TimeProvider.Now)
        {
            this.RemoveUserFromRateLimit(id);
            return false;
        }

        // Entry has not expired yet, rate limit should be enforced
        return true;
    }
}