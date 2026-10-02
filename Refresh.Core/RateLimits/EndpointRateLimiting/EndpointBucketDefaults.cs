using System.Collections.Frozen;
using Refresh.Core.Configuration;

namespace Refresh.Core.RateLimits.EndpointRateLimiting;

public static class EndpointBucketDefaults
{
    public static readonly FrozenDictionary<EndpointBucketId, ConfigRateLimitBucket> Buckets = new Dictionary<EndpointBucketId, ConfigRateLimitBucket>()
    {
        #region Misc
        {EndpointBucketId.Default, new(90, 300, 45)},
        #endregion

        #region Authentication
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
        {EndpointBucketId.ApiGetInstanceInfo, new(240, 30, 180)},
        {EndpointBucketId.ApiGetInstanceStats, new(240, 30, 180)},
        {EndpointBucketId.ApiGetDocumentation, new(240, 30, 180)},
        {EndpointBucketId.ApiGetListOfAnnouncements, new(240, 30, 180)},
        #endregion

        #region Categories
        {EndpointBucketId.ApiGetListOfCategories, new(240, 20, 180)},
        #endregion
        
        #region Levels
        {EndpointBucketId.GameGetSingleLevel, new(240, 200, 180)},

        {EndpointBucketId.ApiGetSingleLevel, new(240, 50, 180)},
        {EndpointBucketId.ApiGetOwnRelationsToLevel, new(240, 50, 180)},
        
        {EndpointBucketId.ApiGetListOfLevels, new(240, 50, 180)},

        {EndpointBucketId.ApiEditLevel, new(300, 20, 180)},

        {EndpointBucketId.DeleteLevel, new(300, 20, 180)},
        {EndpointBucketId.HeartLevel, new(300, 30, 180)},
        {EndpointBucketId.QueueLevel, new(300, 50, 180)},
        {EndpointBucketId.TagLevel, new(300, 10, 180)},
        {EndpointBucketId.RateLevel, new(300, 20, 180)},
        {EndpointBucketId.ApiOverrideLevel, new(300, 20, 180)},
        #endregion

        #region Level Scores
        {EndpointBucketId.ApiGetListOfLevelScores, new(300, 40, 180)},
        {EndpointBucketId.ApiGetSingleLevelScore, new(300, 40, 180)},
        #endregion

        #region Reviews
        {EndpointBucketId.ApiGetListOfReviews, new(300, 40, 180)},
        {EndpointBucketId.ApiGetSingleReview, new(300, 40, 180)},

        {EndpointBucketId.UploadReview, new(300, 12, 180)},
        {EndpointBucketId.RateReview, new(300, 40, 180)},
        {EndpointBucketId.DeleteReview, new(300, 30, 180)},
        #endregion

        #region Comments (both Profile and Level)
        {EndpointBucketId.ApiGetListOfComments, new(300, 40, 180)},
        {EndpointBucketId.ApiGetSingleComment, new(300, 40, 180)},

        {EndpointBucketId.UploadComment, new(300, 18, 180)},
        {EndpointBucketId.RateComment, new(300, 40, 180)},
        {EndpointBucketId.DeleteComment, new(300, 30, 180)},
        #endregion

        #region Photos
        {EndpointBucketId.ApiGetListOfPhotos, new(300, 40, 180)},
        {EndpointBucketId.ApiGetSinglePhoto, new(300, 40, 180)},

        {EndpointBucketId.DeletePhoto, new(300, 30, 180)},
        #endregion

        #region Users
        {EndpointBucketId.ApiGetListOfUsers, new(300, 60, 180)},
        {EndpointBucketId.ApiGetSingleUser, new(300, 60, 180)},
        // this should stay relatively high because currently, both websites will call this to find out whether the user
        // is still authed every time they're refreshed, but also every time the user visits a new page
        {EndpointBucketId.ApiGetOwnUser, new(300, 70, 180)},

        {EndpointBucketId.UpdateUser, new(300, 20, 180)},
        {EndpointBucketId.HeartUser, new(300, 30, 180)},
        #endregion

        #region Assets
        {EndpointBucketId.ApiDownloadAsset, new(240, 500, 120)},
        {EndpointBucketId.ApiDownloadImage, new(240, 250, 120)},

        {EndpointBucketId.ApiGetAssetMetadata, new(240, 250, 120)},
        
        // Regular download limits are this high on both game and API because both the game and third party API clients
        // (e.g. archive_dl) are likely to download many of these at times depending on what level they're trying to load
        // (additionally, adventures can have even more dependencies!)
        {EndpointBucketId.ApiUploadImage, new(300, 20, 180)},
        #endregion

        #region Matching
        // this high because of beta website's fake live updating (sends new request every few seconds), which we have to deal with for now
        {EndpointBucketId.ApiGetListOfRooms, new(240, 90, 120)},
        {EndpointBucketId.ApiGetSingleRoom, new(240, 40, 120)},
        #endregion

        #region Playlists
        {EndpointBucketId.ApiGetListOfPlaylists, new(240, 50, 180)},
        {EndpointBucketId.ApiGetSinglePlaylist, new(240, 50, 180)},

        {EndpointBucketId.CreatePlaylist, new(240, 30, 180)},
        {EndpointBucketId.UpdatePlaylistMetadata, new(240, 30, 180)},
        {EndpointBucketId.UpdatePlaylistContents, new(240, 50, 180)},
        {EndpointBucketId.HeartPlaylist, new(240, 30, 180)},
        {EndpointBucketId.DeletePlaylist, new(240, 30, 180)},
        #endregion

        #region Activity
        {EndpointBucketId.ApiGetActivityPage, new(240, 50, 180)},
        #endregion

        #region Notifications
        {EndpointBucketId.ApiGetListOfNotifications, new(240, 20, 180)},
        {EndpointBucketId.ApiGetSingleNotification, new(240, 20, 180)},
        {EndpointBucketId.ApiDeleteNotification, new(240, 20, 180)},
        #endregion

        #region Contests
        {EndpointBucketId.ApiGetListOfContests, new(240, 20, 180)},
        {EndpointBucketId.ApiGetSingleContest, new(240, 20, 180)},
        #endregion
    }.ToFrozenDictionary();
}