namespace Refresh.Core.RateLimits.EndpointRateLimiting;

// TODO add IDs for all API/game buckets here
// Generally, fetch endpoints should use separate buckets depending on whether they are game/API endpoints,
// while upload/modification/deletion endpoints should share buckets.
public enum EndpointBucketId
{
    #region Misc
    Default,
    #endregion
    
    #region Levels
    GameGetSingleLevel,
    ApiGetSingleLevel,
    #endregion
}