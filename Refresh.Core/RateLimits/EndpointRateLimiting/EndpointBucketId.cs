namespace Refresh.Core.RateLimits.EndpointRateLimiting;

// For comments and explanations regarding these buckets, see EndpointBucketDefaults.
public enum EndpointBucketId
{
    #region Misc
    Default,
    #endregion

    #region Authentication
    GameLogin,
    
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
    GameGetGameConfig,
    GameGetInstanceStats,

    GameGetEula,
    GameGetListOfAnnouncements,
    
    ApiGetInstanceStats,
    ApiGetInstanceInfo,
    ApiGetDocumentation,

    ApiGetListOfAnnouncements,
    #endregion

    #region Categories
    GameGetListOfCategories,
    ApiGetListOfCategories,
    #endregion

    #region Levels
    GamePrepareLevelPublish,
    GameRealLevelPublish,
    
    GameGetListOfLevels,
    GameGetSingleLevel,
    
    ApiGetSingleLevel,
    ApiGetOwnRelationsToLevel,
    ApiGetListOfLevels,

    ApiEditLevel,
    ApiOverrideLevel,

    DeleteLevel,
    HeartLevel,
    QueueLevel,
    TagLevel,
    RateLevel,
    #endregion

    #region Level Scores
    GameGetListOfLevelScores,
    GameUploadLevelScore,
    
    GamePlayLevel,
    
    ApiGetListOfLevelScores,
    ApiGetSingleLevelScore,
    #endregion

    #region Reviews
    GameGetListOfReviews,
    GameGetSingleReview,
    
    ApiGetListOfReviews,
    ApiGetSingleReview,
    
    UploadReview,
    RateReview,
    DeleteReview,
    #endregion

    #region Comments (both Profile and Level)
    GameGetListOfComments, 
    GameGetSingleComment,
    
    ApiGetListOfComments,
    ApiGetSingleComment,
    
    UploadComment,
    RateComment,
    DeleteComment,
    #endregion

    #region Photos
    GameUploadPhoto,
    
    GameGetListOfPhotos,
    GameGetSinglePhoto,
    
    ApiGetListOfPhotos,
    ApiGetSinglePhoto,
    
    DeletePhoto,
    #endregion

    #region Users
    GameGetListOfUsers,
    GameGetSingleUser,
    
    GameUploadFriendData,
    GameSyncUserPrivacySettings,
    
    ApiGetListOfUsers,
    ApiGetSingleUser,
    ApiGetOwnUser,

    UpdateUser,
    HeartUser,
    #endregion

    #region Assets
    GameUploadAsset,
    GameDownloadAsset,
    
    ApiDownloadAsset,
    ApiDownloadImage,
    
    ApiGetAssetMetadata,
    ApiUploadImage,
    #endregion

    #region Matching
    GameUpdateRoomOrGetRooms,
    
    ApiGetListOfRooms,
    ApiGetSingleRoom,
    #endregion

    #region Playlists
    Lbp1GetListOfPlaylists,
    Lbp1GetPlaylistContents,

    Lbp3GetListOfPlaylists,
    Lbp3GetPlaylistContents,
    
    ApiGetListOfPlaylists,
    ApiGetSinglePlaylist,
    
    CreatePlaylist,
    UpdatePlaylistMetadata,
    UpdatePlaylistContents,
    HeartPlaylist,
    DeletePlaylist,
    #endregion

    #region Activity
    GameGetActivityPage,
    
    ApiGetActivityPage,
    #endregion

    #region Notifications
    GameGetListOfNotifications,
    
    ApiGetListOfNotifications,
    ApiGetSingleNotification,
    ApiDeleteNotification,
    #endregion

    #region Contests
    ApiGetListOfContests,
    ApiGetSingleContest,
    #endregion

    #region Moderation
    GameUploadGriefReport,
    GameFilterModeratedAssets,
    GameFilterChatMessage,
    #endregion

    #region Pins
    GameSyncPinProgress,
    #endregion

    #region Challenges
    GameUploadPlayerChallenge,
    GameUploadPlayerChallengeScore,

    GameGetListOfPlayerChallenges,
    GameGetListOfPlayerChallengeScores,
    GameGetSinglePlayerChallengeScore,
    #endregion
}