using MongoDB.Bson;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

[PrimaryKey(nameof(PinId), nameof(PublisherId), nameof(IsBeta), nameof(Platform))]
public partial class PinProgressRelation
{
    /// <summary>
    /// Normally set to the pin's progressType, which is the only pin identifier the game sends
    /// </summary>
    public long PinId { get; set; }
    public int Progress { get; set; }
    [ForeignKey(nameof(PublisherId)), Required]
    public GameUser Publisher { get; set; }
    
    [Required] public ObjectId PublisherId { get; set; }

    public DateTimeOffset FirstPublished { get; set; }
    public DateTimeOffset LastUpdated { get; set; }

    /// <summary>
    /// Whether this pin was achieved in retail LBP2/3/Vita or in a Beta build, to track the progress
    /// for these game groups seperately.
    /// </summary>
    public bool IsBeta { get; set; }

    /// <summary>
    /// Seperates pin progresses per platform, to not let progress transfer inbetween PS3, RPCS3 and especially Vita,
    /// as that's commonly unwanted behaviour. Maybe figure out how to make this be opt-out in the future?
    /// Website = this pin is universal across platforms and games (e.g. LBP.me pin)
    /// </summary>
    public TokenPlatform Platform { get; set; }
}