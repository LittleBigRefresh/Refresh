using System.Xml.Serialization;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.Data;
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

    public static SerializedGameReview? FromOld(GameReview? review, DataContext dataContext)
    {
        if (review == null) 
            return null;
        
        // TODO: fill in review.YourThumb
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
            Thumb = dataContext.Database.GetRatingByUser(review.Level, dataContext.User!)?.ToDPad() ?? 0,
            ThumbsUp = 0,
            ThumbsDown = 0,
            YourThumb = 0,
        };
    }
    
    public static IEnumerable<SerializedGameReview> FromOldList(IEnumerable<GameReview> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}