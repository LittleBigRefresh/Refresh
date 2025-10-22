using System.Xml.Serialization;
using Newtonsoft.Json.Converters;

namespace Refresh.Database.Models.Activity;

/// <summary>
/// Contains both activity and moderation event types
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum EventType : byte
{
    [XmlEnum("publish_level")] LevelUpload = 0,
    [XmlEnum("heart_level")] LevelFavourite = 1,
    [XmlEnum("unheart_level")] LevelUnfavourite = 2,
    [XmlEnum("heart_user")] UserFavourite = 3,
    [XmlEnum("unheart_user")] UserUnfavourite = 4,
    [XmlEnum("play_level")] LevelPlay = 5,
    [XmlEnum("rate_level")] LevelStarRate = 6, // as opposed to dpad rating. unused since we convert stars to dpad
    [XmlEnum("tag_level")] LevelTag = 7,
    [XmlEnum("comment_on_level")] PostLevelComment = 8,
    [XmlEnum("delete_level_comment")] DeleteLevelComment = 9,
    [XmlEnum("upload_photo")] PhotoUpload = 10,
    [XmlEnum("unpublish_level")] LevelUnpublish = 11,
    [XmlEnum("news_post")] NewsPost = 12,
    [XmlEnum("mm_pick_level")] LevelTeamPick = 13,
    [XmlEnum("dpad_rate_level")] LevelRate = 14,
    [XmlEnum("review_level")] LevelReview = 15,
    [XmlEnum("comment_on_user")] PostUserComment = 16,
    [XmlEnum("create_playlist")] PlaylistCreate = 17,
    [XmlEnum("heart_playlist")] PlaylistFavourite = 18,
    [XmlEnum("add_level_to_playlist")] PlaylistAddLevel = 19,
    [XmlEnum("score")] LevelScore = 20,

    // Custom events, mostly additional moderation events.
    
    [XmlEnum("firstlogin")] UserFirstLogin = 127,
    [XmlEnum("create_challenge")] CreateChallenge,
    [XmlEnum("create_challenge_score")] CreateChallengeScore,
    [XmlEnum("add_playlist_to_playlist")] PlaylistAddPlaylist,
    [XmlEnum("delete_playlist")] DeletePlaylist,
    [XmlEnum("delete_user_photos")] DeleteUserPhotos,
    [XmlEnum("delete_photo")] DeletePhoto,
    [XmlEnum("delete_review")] DeleteReview,
    [XmlEnum("delete_user_comment")] DeleteUserComment, 
    [XmlEnum("create_contest")] CreateContest,
    [XmlEnum("delete_contest")] DeleteContest,
    [XmlEnum("un_mm_pick_level")] LevelUnTeamPick,
    [XmlEnum("delete_score")] DeleteScore,
    [XmlEnum("delete_user_scores")] DeleteUserScores,
    // Useful if the user has submitted multiple cheated scores on a single level, only one is showing as a highscore at a time, 
    // but staff doesn't want to snipe the user's other level scores because they mostly seem legitimate
    [XmlEnum("delete_user_level_scores")] DeleteUserLevelScores, 
    [XmlEnum("moderate_asset")] ModerateAsset,
    [XmlEnum("unmoderate_asset")] UnmoderateAsset,
    [XmlEnum("restrict_user")] RestrictUser,
    [XmlEnum("ban_user")] BanUser,
    [XmlEnum("delete_user")] DeleteUser,
    [XmlEnum("pardon_user")] PardonUser,
    [XmlEnum("update_user")] UpdateUser,
    [XmlEnum("delete_challenge")] DeleteChallenge,
    [XmlEnum("delete_user_challenges")] DeleteUserChallenges,
    [XmlEnum("delete_challenge_score")] DeleteChallengeScore,
    [XmlEnum("delete_user_challenge_scores")] DeleteUserChallengeScores,
}