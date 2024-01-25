using System.Xml.Serialization;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Reviews;

[XmlRoot("review")]
[XmlType("review")]
public class SerializedGameReview : IDataConvertableFrom<SerializedGameReview, GameReview>
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
            Id = review.SequentialId,
            Slot = new GameReviewSlot
            {
                SlotType = review.Level.First().Source.ToGameType(),
                SlotId = review.Level.First().LevelId,
            },
            Reviewer = review.Publisher.Username,
            Timestamp = review.Timestamp,
            Labels = review.Labels,
            Deleted = false,
            DeletedBy = ReviewDeletedBy.None,
            Text = review.Text,
            Thumb = review.Level.First().Ratings.FirstOrDefault(r => r.User == review.Publisher)?.RatingType.ToDPad() ?? 0,
            ThumbsUp = 0,
            ThumbsDown = 0,
            YourThumb = 0,
        };
    }

    public void FillInExtraData(GameDatabaseContext database, GameUser user)
    {
        //TODO: fill in this.YourThumb
    }
    
    public static IEnumerable<SerializedGameReview> FromOldList(IEnumerable<GameReview> oldList) => oldList.Select(FromOld).ToList()!;
}