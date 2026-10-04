namespace Refresh.Core.RateLimits.EndpointRateLimiting;

// For comments and explanations regarding these buckets, see EndpointBucketDefaults.
public enum EndpointBucketId
{
    #region Misc
    Default,
    #endregion

    #region Authentication
    GameLogin,
    #endregion

    #region Instance
    GameGetGameConfig,
    GameGetInstanceStats,

    GameGetEula,
    GameGetListOfAnnouncements,
    #endregion

    #region Categories
    GameGetListOfCategories,
    #endregion

    #region Levels
    GameGetListOfLevels,
    GameGetSingleLevel,
    ApiGetSingleLevel,

    GamePrepareLevelPublish,
    GameRealLevelPublish,

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
    #endregion

    #region Reviews
    GameGetListOfReviews,
    GameGetSingleReview,

    UploadReview,
    RateReview,
    DeleteReview,
    #endregion

    #region Comments (both Profile and Level)
    GameGetListOfComments, 
    GameGetSingleComment,

    UploadComment,
    RateComment,
    DeleteComment,
    #endregion

    #region Photos
    GameGetListOfPhotos,
    GameGetSinglePhoto,

    GameUploadPhoto,
    DeletePhoto,
    #endregion

    #region Users
    GameGetListOfUsers,
    GameGetSingleUser,

    UpdateUser,
    GameUploadFriendData,
    GameSyncUserPrivacySettings,
    HeartUser,
    #endregion

    #region Assets
    GameUploadAsset,
    GameDownloadAsset,
    #endregion

    #region Matching
    GameUpdateRoomOrGetRooms,
    #endregion

    #region Playlists
    Lbp1GetListOfPlaylists,
    Lbp1GetPlaylistContents,

    Lbp3GetListOfPlaylists,
    Lbp3GetPlaylistContents,

    CreatePlaylist,
    UpdatePlaylistMetadata,
    UpdatePlaylistContents,
    HeartPlaylist,
    DeletePlaylist,
    #endregion

    #region Activity
    GameGetActivityPage,
    #endregion

    #region Notifications
    GameGetListOfNotifications,
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