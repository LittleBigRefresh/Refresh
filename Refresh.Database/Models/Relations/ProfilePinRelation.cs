using MongoDB.Bson;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

[PrimaryKey(nameof(Index), nameof(PublisherId), nameof(Game), nameof(Platform))]
public partial class ProfilePinRelation
{
    public long PinId { get; set; }
    
    [ForeignKey(nameof(PublisherId))]
    [Required] public GameUser Publisher { get; set; }
    
    [Required] public ObjectId PublisherId { get; set; }

    /// <summary>
    /// Whether to show as first, second or third pin in-game
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// The game to show this profile pin on, to allow different sets of profile pins per game
    /// </summary>
    public TokenGame Game { get; set; }

    /// <summary>
    /// Since pin progresses are split per platforms now, we also have to split profile pins aswell
    /// </summary>
    public TokenPlatform Platform { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}