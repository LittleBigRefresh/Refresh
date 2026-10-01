using System.Collections.Frozen;
using Refresh.Core.Configuration;

namespace Refresh.Core.RateLimits.EndpointRateLimiting;

// TODO add IDs for all API buckets here
// TODO separate buckets for PSP for certain endpoints, since the ones in question are spammed by PSP in certain cases.
// Generally, fetch endpoints should use separate buckets depending on whether they are game/API endpoints,
// while upload/modification/deletion endpoints should share buckets.
public static class EndpointBucketDefaults
{
    public static readonly FrozenDictionary<EndpointBucketId, ConfigRateLimitBucket> Buckets = new Dictionary<EndpointBucketId, ConfigRateLimitBucket>()
    {
        #region Misc
        {EndpointBucketId.Default, new(90, 300, 45)}, 
        #endregion

        #region Authentication
        {EndpointBucketId.GameLogin, new(300, 10, 300)},
        #endregion

        #region Instance
        {EndpointBucketId.GameGetGameConfig, new(240, 30, 180)},
        {EndpointBucketId.GameGetInstanceStats, new(240, 30, 180)},
        {EndpointBucketId.GameGetEula, new(240, 30, 180)},
        {EndpointBucketId.GameGetListOfAnnouncements, new(240, 30, 180)},
        #endregion

        #region Categories
        // LBP3 spams if fetching Genre categories fails, so keep a little higher than reasonable
        {EndpointBucketId.GameGetListOfCategories, new(240, 40, 180)},
        #endregion
        
        #region Levels
        {EndpointBucketId.GameGetListOfLevels, new(240, 50, 180)},
        
        // Game sometimes requests many levels in bursts.
        {EndpointBucketId.GameGetSingleLevel, new(240, 200, 180)},
        {EndpointBucketId.ApiGetSingleLevel, new(240, 50, 180)},
        
        // Should use separate buckets for each publish endpoint so we don't end up allowing /startPublish but blocking /publish
        // Also, keeping these buckets separate might avoid confusion by the server owner where they might wonder why 
        // it takes them less publish attempts to hit the limit than the actual limit they've set.
        {EndpointBucketId.GamePrepareLevelPublish, new(600, 20, 360)},
        {EndpointBucketId.GameRealLevelPublish, new(600, 20, 360)},
        
        {EndpointBucketId.DeleteLevel, new(300, 20, 180)},
        {EndpointBucketId.HeartLevel, new(300, 30, 180)},
        {EndpointBucketId.QueueLevel, new(300, 50, 180)}, // lbp3 has a hacky feature where you can mass-queue levels from playlists
        {EndpointBucketId.TagLevel, new(300, 10, 180)},
        {EndpointBucketId.RateLevel, new(300, 20, 180)},
        #endregion

        #region Level Scores
        {EndpointBucketId.GameGetListOfLevelScores, new(300, 40, 180)},
        {EndpointBucketId.GameUploadLevelScore, new(300, 30, 180)},

        {EndpointBucketId.GamePlayLevel, new(300, 30, 180)},
        #endregion

        #region Reviews
        {EndpointBucketId.GameGetListOfReviews, new(300, 40, 180)},
        {EndpointBucketId.GameGetSingleReview, new(300, 40, 180)},

        {EndpointBucketId.UploadReview, new(300, 12, 180)},
        {EndpointBucketId.RateReview, new(300, 40, 180)},
        {EndpointBucketId.DeleteReview, new(300, 30, 180)},
        #endregion

        #region Comments (both Profile and Level)
        {EndpointBucketId.GameGetListOfComments, new(300, 40, 180)},
        {EndpointBucketId.GameGetSingleComment, new(300, 40, 180)},

        {EndpointBucketId.UploadComment, new(300, 18, 180)},
        {EndpointBucketId.RateComment, new(300, 40, 180)},
        {EndpointBucketId.DeleteComment, new(300, 30, 180)},
        #endregion

        #region Photos
        {EndpointBucketId.GameGetListOfPhotos, new(300, 40, 180)},
        {EndpointBucketId.GameGetSinglePhoto, new(300, 40, 180)},
        
        {EndpointBucketId.GameUploadPhoto, new(300, 25, 180)},
        {EndpointBucketId.DeletePhoto, new(300, 30, 180)},
        #endregion

        #region Users
        {EndpointBucketId.GameGetListOfUsers, new(300, 60, 180)},
        {EndpointBucketId.GameGetSingleUser, new(300, 60, 180)},

        {EndpointBucketId.UpdateUser, new(300, 20, 180)},
        {EndpointBucketId.GameUploadFriendData, new(240, 6, 180)},
        {EndpointBucketId.GameSyncUserPrivacySettings, new(300, 10, 180)},
        {EndpointBucketId.HeartUser, new(300, 30, 180)},
        #endregion

        #region Assets
        // Regular download limits should be high on both game and API because both the game and third party API clients
        // (e.g. archive_dl) are likely to download many of these at times depending on what level they're trying to load
        // (additionally, adventures will usually have even more dependencies than regular levels!)
        {EndpointBucketId.GameUploadAsset, new(300, 150, 180)},
        {EndpointBucketId.GameDownloadAsset, new(240, 500, 120)},
        #endregion

        #region Matching
        {EndpointBucketId.GameUpdateRoomOrGetRooms, new(240, 30, 120)},
        #endregion

        #region Playlists
        {EndpointBucketId.Lbp1GetListOfPlaylists, new(240, 50, 180)},
        {EndpointBucketId.Lbp1GetPlaylistContents, new(240, 50, 180)},
        
        // LBP3 doesn't cache playlists or playlist levels, so it'll request these far more often than LBP1.
        // Also, it will spam the levels endpoint for every playlist from the playlist response for a particular user.
        // Playlists in general are very messy and buggy in LBP3, they have a property on their playlist response describing its
        // preview level icons, but they instead spam these level requests to get the icons instead of using the playlist property.
        {EndpointBucketId.Lbp3GetListOfPlaylists, new(240, 50, 180)},
        {EndpointBucketId.Lbp3GetPlaylistContents, new(240, 90, 180)},

        {EndpointBucketId.CreatePlaylist, new(240, 30, 180)},
        {EndpointBucketId.UpdatePlaylistMetadata, new(240, 30, 180)},
        {EndpointBucketId.UpdatePlaylistContents, new(240, 50, 180)},
        {EndpointBucketId.HeartPlaylist, new(240, 30, 180)},
        {EndpointBucketId.DeletePlaylist, new(240, 30, 180)},
        #endregion

        #region Activity
        {EndpointBucketId.GameGetActivityPage, new(240, 50, 180)},
        #endregion

        #region Notifications
        {EndpointBucketId.GameGetListOfNotifications, new(240, 20, 180)},
        #endregion

        #region Moderation
        {EndpointBucketId.GameUploadGriefReport, new(300, 10, 240)},
        {EndpointBucketId.GameFilterModeratedAssets, new(300, 60, 180)},
        
        // this high just because of adventure uploading,
        // TODO rate-limit specific chat commands separately (would need condition in endpoint method)
        {EndpointBucketId.GameFilterChatMessage, new(60, 900, 30)},
        #endregion

        #region Pins
        {EndpointBucketId.GameSyncPinProgress, new(240, 12, 180)},
        #endregion

        #region Challenges
        {EndpointBucketId.GameUploadPlayerChallenge, new(240, 8, 180)},
        {EndpointBucketId.GameUploadPlayerChallengeScore, new(240, 16, 180)},

        {EndpointBucketId.GameGetListOfPlayerChallenges, new(240, 20, 180)},
        {EndpointBucketId.GameGetListOfPlayerChallengeScores, new(240, 50, 180)},
        {EndpointBucketId.GameGetSinglePlayerChallengeScore, new(240, 40, 180)},
        #endregion
    }.ToFrozenDictionary();
}