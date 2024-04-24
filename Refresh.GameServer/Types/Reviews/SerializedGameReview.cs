using System.Xml.Serialization;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Reviews;

[XmlRoot("review")]
[XmlType("review")]
public class SerializedGameReview
{
    [XmlElement("id")] 
    public int Id { get; set; }
    
    [XmlElement("slot_id")]
    public GameReviewSlot Slot { get; set; }
    
    [XmlElement("reviewer")]
    public string Reviewer { get; set; }

    [XmlElement("timestamp")]
    public long Timestamp { get; set; }

    [XmlElement("labels")]
    public string Labels { get; set; } = "";

    [XmlElement("deleted")]
    public bool Deleted { get; set; } = false;

    [XmlElement("deleted_by")]
    public ReviewDeletedBy DeletedBy { get; set; } = ReviewDeletedBy.None;

    [XmlElement("text")]
    public string Text { get; set; } = "";
    
    /// <summary>
    /// The rating the user has on the level.
    /// </summary>
    [XmlElement("thumb")]
    public int Thumb { get; set; }

    [XmlElement("thumbsup")]
    public int ThumbsUp { get; set; }

    [XmlElement("thumbsdown")]
    public int ThumbsDown { get; set; }
    
    [XmlElement("yourthumb")]
    public RatingType YourThumb { get; set; }

    public static SerializedGameReview? FromOld(GameReview? review)
    {
        if (review == null) 
            return null;
        
        return new SerializedGameReview
        {
            Id = review.ReviewId,
            Slot = new GameReviewSlot
            {
                SlotType = review.Level.Source.ToGameType(),
                SlotId = review.Level.LevelId,
            },
            Reviewer = review.Publisher.Username,
            Timestamp = review.PostedAt.ToUnixTimeMilliseconds(),
            Labels = review.Labels,
            Deleted = false,
            DeletedBy = ReviewDeletedBy.None,
            Text = review.Content,
            Thumb = 0,
            ThumbsUp = 0,
            ThumbsDown = 0,
            YourThumb = 0,
        };
    }

    public void FillInExtraData(GameDatabaseContext database, GameUser user)
    {
        // TODO: no
        GameLevel? level = database.GetLevelByIdAndType(Slot.SlotType, Slot.SlotId);
        if (level == null) return;
        
        // TODO: no
        GameUser? reviewer = database.GetUserByUsername(this.Reviewer);
        if (reviewer == null) return;
        
        RatingType? yourRating = database.GetRatingByUser(level, user);
        this.YourThumb = yourRating ?? RatingType.Neutral;
    }
    
    public static IEnumerable<SerializedGameReview> FromOldList(IEnumerable<GameReview> oldList) => oldList.Select(FromOld).ToList()!;
}