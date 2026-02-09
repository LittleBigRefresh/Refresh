using Newtonsoft.Json.Converters;

namespace Refresh.Database.Models.Moderation;

[JsonConverter(typeof(StringEnumConverter))]
public enum ModerationActionType : byte
{
    // Users
    UserModification,
    UserDeletion,
    UserPunishment,
    UserPardon,
    PinProgressDeletion,

    // Levels
    LevelModification,
    LevelDeletion,

    // Playlists
    PlaylistModification,
    PlaylistDeletion,

    // Photos
    PhotoDeletion,
    PhotosByUserDeletion,

    // Scores
    ScoreDeletion,
    ScoresByUserForLevelDeletion,
    ScoresByUserDeletion,

    // Reviews
    ReviewDeletion,
    ReviewsByUserDeletion,
    
    // Comments
    LevelCommentDeletion,
    LevelCommentsByUserDeletion,
    ProfileCommentDeletion,
    ProfileCommentsByUserDeletion,

    // Assets
    BlockAsset,
    UnblockAsset,

    // Challenges
    ChallengeDeletion,
    ChallengesByUserDeletion,
    ChallengeScoreDeletion,
    ChallengeScoresByUserDeletion,
}