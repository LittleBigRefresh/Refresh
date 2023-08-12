using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Refresh.GameServer.Types.Activity;

[JsonConverter(typeof(StringEnumConverter))]
public enum EventType
{
    [XmlEnum("publish_level")]
    Level_Upload = 0,
    [XmlEnum("heart_level")]
    Level_Favourite = 1,
    [XmlEnum("unheart_level")]
    Level_Unfavourite = 2,
    [XmlEnum("heart_user")]
    User_Favourite = 3,
    [XmlEnum("unheart_user")]
    User_Unfavourite = 4,
    [XmlEnum("play_level")]
    Level_Play = 5,
    // [XmlEnum("rate_level")]
    // Level_Rate = 6,
    [XmlEnum("tag_level")]
    Level_Tag = 7,
    // [XmlEnum("comment_on_level")]
    // PostLevelComment = 8,
    // [XmlEnum("delete_level_comment")]
    // DeleteLevelComment = 9,
    // [XmlEnum("upload_photo")]
    // Photo_Upload = 10,
    // [XmlEnum("unpublish_level")]
    // Level_Unpublish = 11,
    // [XmlEnum("news_post")]
    // News_Post = 12,
    [XmlEnum("mm_pick_level")]
    Level_TeamPick = 13,
    [XmlEnum("dpad_rate_level")]
    RateLevelRelation_Create = 14,
    [XmlEnum("review_level")]
    Level_Review = 15,
    // [XmlEnum("comment_on_user")]
    // PostUserComment = 16,
    // [XmlEnum("create_playlist")]
    // Playlist_Create = 17,
    // [XmlEnum("heart_playlist")]
    // Playlist_Favourite = 18,
    // [XmlEnum("add_level_to_playlist")]
    // Playlist_AddLevel = 19,
    [XmlEnum("score")]
    SubmittedScore_Create = 20, // FIXME: this name is shit
}