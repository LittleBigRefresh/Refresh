using Newtonsoft.Json.Converters;

namespace Refresh.Database.Models.Moderation;

[JsonConverter(typeof(StringEnumConverter))]
public enum ModerationActionType : byte
{
    // Users
    UserModification,
    UserDeletion,

    // Levels
    LevelModification,
    LevelDeletion,

    // Playlists
    PlaylistModification,
    PlaylistDeletion,

    // Scores
    ScoreDeletion,
    ScoresByUserForLevelDeletion,
    ScoresByUserDeletion,

    // Reviews
    ReviewDeletion,
    ReviewsByUserByUserDeletion,
    
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