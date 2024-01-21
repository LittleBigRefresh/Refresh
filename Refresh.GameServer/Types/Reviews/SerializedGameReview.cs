using System.Xml.Serialization;
using Realms;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Reviews;

[XmlRoot("deleted_by")]
public enum ReviewDeletedBy
{
    [XmlEnum(Name = "none")]
    None,
    [XmlEnum(Name = "moderator")]
    Moderator,
    [XmlEnum(Name = "level_author")]
    LevelAuthor,
}

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

    [XmlElement("thumb")]
    public int Thumb { get; set; }

    [XmlElement("thumbsup")]
    public int ThumbsUp { get; set; }

    [XmlElement("thumbsdown")]
    public int ThumbsDown { get; set; }
    
    [XmlElement("yourthumb")]
    public RatingType YourThumb { get; set; }

    public SerializedGameReview FromOld(GameReview review)
    {
        return new SerializedGameReview
        {
            Id = review.SequentialId,
            Slot = new GameReviewSlot
            {
                SlotType = review.Level.Source.ToGameType(),
                SlotId = review.Level.LevelId,
            },
            Reviewer = review.Publisher.Username,
            Timestamp = review.Timestamp,
            Labels = review.Labels,
            Deleted = review.Deleted,
            DeletedBy = review.DeletedBy,
            //If the review is deleted, dont send the review text
            Text = review.Deleted ? "" : review.Text,
            Thumb = 0,
            ThumbsUp = 0,
            ThumbsDown = 0,
            YourThumb = 0,
        };
    }
}