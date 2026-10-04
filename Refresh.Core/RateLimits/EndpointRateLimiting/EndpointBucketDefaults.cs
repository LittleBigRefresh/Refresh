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
      
        {EndpointBucketId.ApiLogin, new(300, 10, 300)},
        {EndpointBucketId.ApiRegister, new(3600, 10, 1800)},

        {EndpointBucketId.ApiRequestEmail, new(300, 10, 300)},
        {EndpointBucketId.ApiVerifyEmailAddress, new(300, 10, 300)},
        {EndpointBucketId.ApiResetPassword, new(300, 10, 300)},

        {EndpointBucketId.ApiGetListOfIpAddresses, new(300, 30, 240)},
        {EndpointBucketId.ApiApproveOrDenyIpAddress, new(300, 30, 240)},

        {EndpointBucketId.ApiDeleteOwnUser, new(600, 6, 480)},
        #endregion

        #region Instance
        {EndpointBucketId.GameGetGameConfig, new(240, 30, 180)},
        {EndpointBucketId.GameGetInstanceStats, new(240, 30, 180)},
        {EndpointBucketId.GameGetEula, new(240, 30, 180)},
        {EndpointBucketId.GameGetListOfAnnouncements, new(240, 30, 180)},
      
        {EndpointBucketId.ApiGetInstanceInfo, new(240, 30, 180)},
        {EndpointBucketId.ApiGetInstanceStats, new(240, 30, 180)},
        {EndpointBucketId.ApiGetDocumentation, new(240, 30, 180)},
        {EndpointBucketId.ApiGetListOfAnnouncements, new(240, 30, 180)},
        #endregion

        #region Categories
        // LBP3 spams if fetching Genre categories fails, so keep a little higher than reasonable
        {EndpointBucketId.GameGetListOfCategories, new(240, 40, 180)},
        {EndpointBucketId.ApiGetListOfCategories, new(240, 20, 180)},
        #endregion
        
        #region Levels
        {EndpointBucketId.GameGetListOfLevels, new(240, 50, 180)},
        
        // Game sometimes requests many levels in bursts.
        {EndpointBucketId.GameGetSingleLevel, new(240, 200, 180)},
      
        // Should use separate buckets for each publish endpoint so we don't end up allowing /startPublish but blocking /publish
        // Also, keeping these buckets separate might avoid confusion by the server owner where they might wonder why 
        // it takes them less publish attempts to hit the limit than the actual limit they've set.
        {EndpointBucketId.GamePrepareLevelPublish, new(600, 20, 360)},
        {EndpointBucketId.GameRealLevelPublish, new(600, 20, 360)},

        {EndpointBucketId.ApiGetSingleLevel, new(240, 50, 180)},
        {EndpointBucketId.ApiGetOwnRelationsToLevel, new(240, 50, 180)},
        {EndpointBucketId.ApiGetListOfLevels, new(240, 50, 180)},

        {EndpointBucketId.ApiEditLevel, new(300, 20, 180)},
        {EndpointBucketId.ApiOverrideLevel, new(300, 20, 180)},

        {EndpointBucketId.DeleteLevel, new(300, 20, 180)},
        {EndpointBucketId.HeartLevel, new(300, 30, 180)},
        {EndpointBucketId.QueueLevel, new(300, 50, 180)},
        {EndpointBucketId.TagLevel, new(300, 10, 180)},
        {EndpointBucketId.RateLevel, new(300, 20, 180)},
        #endregion

        #region Level Scores
        {EndpointBucketId.GameGetListOfLevelScores, new(300, 40, 180)},
        {EndpointBucketId.GameUploadLevelScore, new(300, 30, 180)},

        {EndpointBucketId.GamePlayLevel, new(300, 30, 180)},
      
        {EndpointBucketId.ApiGetListOfLevelScores, new(300, 40, 180)},
        {EndpointBucketId.ApiGetSingleLevelScore, new(300, 40, 180)},
        #endregion

        #region Reviews
        {EndpointBucketId.GameGetListOfReviews, new(300, 40, 180)},
        {EndpointBucketId.GameGetSingleReview, new(300, 40, 180)},

        {EndpointBucketId.ApiGetListOfReviews, new(300, 40, 180)},
        {EndpointBucketId.ApiGetSingleReview, new(300, 40, 180)},

        {EndpointBucketId.UploadReview, new(300, 12, 180)},
        {EndpointBucketId.RateReview, new(300, 40, 180)},
        {EndpointBucketId.DeleteReview, new(300, 30, 180)},
        #endregion

        #region Comments (both Profile and Level)
        {EndpointBucketId.GameGetListOfComments, new(300, 40, 180)},
        {EndpointBucketId.GameGetSingleComment, new(300, 40, 180)},
      
        {EndpointBucketId.ApiGetListOfComments, new(300, 40, 180)},
        {EndpointBucketId.ApiGetSingleComment, new(300, 40, 180)},

        {EndpointBucketId.UploadComment, new(300, 18, 180)},
        {EndpointBucketId.RateComment, new(300, 40, 180)},
        {EndpointBucketId.DeleteComment, new(300, 30, 180)},
        #endregion

        #region Photos
        {EndpointBucketId.GameUploadPhoto, new(300, 25, 180)},
        {EndpointBucketId.GameGetListOfPhotos, new(300, 40, 180)},
        {EndpointBucketId.GameGetSinglePhoto, new(300, 40, 180)},
      
        {EndpointBucketId.ApiGetListOfPhotos, new(300, 40, 180)},
        {EndpointBucketId.ApiGetSinglePhoto, new(300, 40, 180)},
        
        {EndpointBucketId.DeletePhoto, new(300, 30, 180)},
        #endregion

        #region Users
        {EndpointBucketId.GameUploadFriendData, new(240, 6, 180)},
        {EndpointBucketId.GameSyncUserPrivacySettings, new(300, 10, 180)},
      
        {EndpointBucketId.GameGetListOfUsers, new(300, 60, 180)},
        {EndpointBucketId.GameGetSingleUser, new(300, 60, 180)},
      
        {EndpointBucketId.ApiGetListOfUsers, new(300, 60, 180)},
        {EndpointBucketId.ApiGetSingleUser, new(300, 60, 180)},
        // this should stay relatively high because currently, both websites will call this to find out whether the user
        // is still authed every time they're refreshed, but also every time the user visits a new page
        {EndpointBucketId.ApiGetOwnUser, new(300, 70, 180)},

        {EndpointBucketId.UpdateUser, new(300, 20, 180)},
        {EndpointBucketId.HeartUser, new(300, 30, 180)},
        #endregion

        #region Assets
        // Regular download limits are this high on both game and API because both the game and third party API clients
        // (e.g. archive_dl) are likely to download many of these at times depending on what level they're trying to load
        // (additionally, adventures can have even more dependencies!)
        {EndpointBucketId.GameDownloadAsset, new(240, 500, 120)},
        {EndpointBucketId.GameUploadAsset, new(300, 150, 180)},
      
        {EndpointBucketId.ApiDownloadAsset, new(240, 500, 120)},
        {EndpointBucketId.ApiDownloadImage, new(240, 250, 120)},

        {EndpointBucketId.ApiGetAssetMetadata, new(240, 250, 120)},
        {EndpointBucketId.ApiUploadImage, new(300, 20, 180)},
        #endregion

        #region Matching
        {EndpointBucketId.GameUpdateRoomOrGetRooms, new(240, 30, 120)},
      
        // this high because of beta website's fake live updating (sends new request every few seconds), which we have to deal with for now
        {EndpointBucketId.ApiGetListOfRooms, new(240, 90, 120)},
        {EndpointBucketId.ApiGetSingleRoom, new(240, 40, 120)},
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
      
        {EndpointBucketId.ApiGetListOfPlaylists, new(240, 50, 180)},
        {EndpointBucketId.ApiGetSinglePlaylist, new(240, 50, 180)},

        {EndpointBucketId.CreatePlaylist, new(240, 30, 180)},
        {EndpointBucketId.UpdatePlaylistMetadata, new(240, 30, 180)},
        {EndpointBucketId.UpdatePlaylistContents, new(240, 50, 180)},
        {EndpointBucketId.HeartPlaylist, new(240, 30, 180)},
        {EndpointBucketId.DeletePlaylist, new(240, 30, 180)},
        #endregion

        #region Activity
        {EndpointBucketId.GameGetActivityPage, new(240, 50, 180)},
        {EndpointBucketId.ApiGetActivityPage, new(240, 50, 180)},
        #endregion

        #region Contests
        {EndpointBucketId.ApiGetListOfContests, new(240, 20, 180)},
        {EndpointBucketId.ApiGetSingleContest, new(240, 20, 180)},
        #endregion

        #region Notifications
        {EndpointBucketId.GameGetListOfNotifications, new(240, 20, 180)},
      
        {EndpointBucketId.ApiGetListOfNotifications, new(240, 20, 180)},
        {EndpointBucketId.ApiGetSingleNotification, new(240, 20, 180)},
        {EndpointBucketId.ApiDeleteNotification, new(240, 20, 180)},
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