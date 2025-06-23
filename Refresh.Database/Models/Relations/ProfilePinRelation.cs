using MongoDB.Bson;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

[PrimaryKey(nameof(PinId), nameof(PublisherId))]
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
    public TokenGame Game
    {
        get => (TokenGame)this._Game;
        set => this._Game = (int)value;
    }
    public int _Game { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}