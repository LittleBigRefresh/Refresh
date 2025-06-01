using Bunkum.Core.Services;
using MongoDB.Bson;
using NotEnoughLogs;

namespace Refresh.Core.Services;

public class ChallengeGhostRateLimitService : EndpointService
{
    public ChallengeGhostRateLimitService(Logger logger, TimeProviderService timeProviderService) : base(logger)
    {
        this._timeProviderService = timeProviderService;
    }

    private readonly TimeProviderService _timeProviderService;

    /// <summary>
    /// Contains the IDs of users whose challenge ghost download requests made by LBP Hub should be temporarily blocked.
    /// Nessesary to work around an LBP Hub bug described in <see cref="Endpoints.Game.ChallengeEndpoints.GetContextualScoresForChallenge"/>
    /// </summary>
    private readonly Dictionary<ObjectId, DateTimeOffset?> _challengeGhostRateLimitedUsers = new();

    /// <summary>
    /// Adds the given user ID to a dictionary containing the IDs of users whose challenge ghost 
    /// download requests made by LBP Hub should be temporarily blocked
    /// </summary>
    /// <param name="id">The user ID</param>
    public void AddUserToChallengeGhostRateLimit(ObjectId id)
    {
        // Unconditionally add the user to the set
        this._challengeGhostRateLimitedUsers.Add(id, this._timeProviderService.TimeProvider.Now);
    }

    /// <summary>
    /// Removes the given user ID from a dictionary containing the IDs of users whose challenge ghost 
    /// download requests made by LBP Hub should be temporarily blocked
    /// </summary>
    /// <param name="id">The user ID</param>
    public void RemoveUserFromChallengeGhostRateLimit(ObjectId id)
    {
        // Unconditionally remove the user from the set
        this._challengeGhostRateLimitedUsers.Remove(id);
    }

    /// <summary>
    /// If the dictionary (containing the user IDs of users whose LBP Hub games' challenge ghost download requests to block)
    /// contains an entry for the given user ID, and it's DateTimeOffset has not "expired" yet (less than 15 seconds away from now),
    /// return true, else false. If the entry has expired, it will automatically be removed from the dictionary by this method.
    /// </summary>
    /// <param name="id">The user ID</param>
    public bool IsUserChallengeGhostRateLimited(ObjectId id)
    {
        KeyValuePair<ObjectId, DateTimeOffset?> usersRateLimit = this._challengeGhostRateLimitedUsers
            .FirstOrDefault(k => k.Key == id, new(id, null));
        
        // No entry
        if (usersRateLimit.Value == null)
            return false;
        
        DateTimeOffset expirationDate = (DateTimeOffset)usersRateLimit.Value;

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