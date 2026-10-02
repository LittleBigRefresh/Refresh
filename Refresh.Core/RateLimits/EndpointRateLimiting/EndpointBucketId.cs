namespace Refresh.Core.RateLimits.EndpointRateLimiting;

public enum EndpointBucketId
{
    #region Misc
    Default,
    #endregion

    #region Authentication
    ApiLogin,
    ApiRegister,
    ApiRequestEmail,
    ApiVerifyEmailAddress,
    ApiResetPassword,
    ApiGetListOfIpAddresses,
    ApiApproveOrDenyIpAddress,
    ApiDeleteOwnUser,
    #endregion

    #region Instance
    ApiGetInstanceStats,
    ApiGetInstanceInfo,
    ApiGetDocumentation,

    ApiGetListOfAnnouncements,
    #endregion

    #region Categories
    ApiGetListOfCategories,
    #endregion
    
    #region Levels
    GameGetSingleLevel,
    ApiGetSingleLevel,
    ApiGetOwnRelationsToLevel,
    
    ApiGetListOfLevels,

    ApiEditLevel,

    DeleteLevel,
    HeartLevel,
    QueueLevel,
    TagLevel,
    RateLevel,
    ApiOverrideLevel,
    #endregion

    #region Level Scores
    ApiGetListOfLevelScores,
    ApiGetSingleLevelScore,
    #endregion

    #region Reviews
    ApiGetListOfReviews,
    ApiGetSingleReview,

    UploadReview,
    RateReview,
    DeleteReview,
    #endregion

    #region Comments (both Profile and Level)
    ApiGetListOfComments,
    ApiGetSingleComment,

    UploadComment,
    RateComment,
    DeleteComment,
    #endregion

    #region Photos
    ApiGetListOfPhotos,
    ApiGetSinglePhoto,

    DeletePhoto,
    #endregion

    #region Users
    ApiGetListOfUsers,
    ApiGetSingleUser,
    ApiGetOwnUser,

    UpdateUser,
    HeartUser,
    #endregion

    #region Assets
    ApiDownloadAsset,
    ApiDownloadImage,
    ApiGetAssetMetadata,
    ApiUploadImage,
    #endregion

    #region Matching
    ApiGetListOfRooms,
    ApiGetSingleRoom,
    #endregion

    #region Playlists
    ApiGetListOfPlaylists,
    ApiGetSinglePlaylist,

    CreatePlaylist,
    UpdatePlaylistMetadata,
    UpdatePlaylistContents,
    HeartPlaylist,
    DeletePlaylist,
    #endregion

    #region Activity
    ApiGetActivityPage,
    #endregion

    #region Notifications
    ApiGetListOfNotifications,
    ApiGetSingleNotification,
    ApiDeleteNotification,
    #endregion

    #region Contests
    ApiGetListOfContests,
    ApiGetSingleContest,
    #endregion
}