using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity;

public enum EventType
{
    [XmlEnum("publish_level")]
    LevelUpload = 0,
    [XmlEnum("heart_level")]
    LevelFavourite = 1,
    [XmlEnum("unheart_level")]
    LevelUnfavourite = 2,
    [XmlEnum("heart_user")]
    UserFavourite = 3,
    [XmlEnum("unheart_user")]
    UserUnfavourite = 4,
    [XmlEnum("play_level")]
    LevelPlay = 5,
    [XmlEnum("rate_level")]
    LevelRate = 6,
    [XmlEnum("tag_level")]
    LevelTag = 7,
    // [XmlEnum("comment_on_level")]
    // PostLevelComment = 8,
    // [XmlEnum("delete_level_comment")]
    // DeleteLevelComment = 9,
    // [XmlEnum("upload_photo")]
    // PhotoUpload = 10,
    // [XmlEnum("unpublish_level")]
    // LevelUnpublish = 11,
    // [XmlEnum("news_post")]
    // NewsPost = 12,
    [XmlEnum("mm_pick_level")]
    LevelTeamPick = 13,
    [XmlEnum("dpad_rate_level")]
    LevelDpadRate = 14,
    [XmlEnum("review_level")]
    LevelReview = 15,
    // [XmlEnum("comment_on_user")]
    // PostUserComment = 16,
    // [XmlEnum("create_playlist")]
    // PlaylistCreate = 17,
    // [XmlEnum("heart_playlist")]
    // PlaylistFavourite = 18,
    // [XmlEnum("add_level_to_playlist")]
    // PlaylistAddLevel = 19,
}