using MongoDB.Bson;
using Refresh.Database.Models.Users;

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
#endif

namespace Refresh.Database.Models.Relations;

#nullable disable

[PrimaryKey(nameof(PinId), nameof(PublisherId))]
public partial class PinProgressRelation : IRealmObject
{
    /// <summary>
    /// Normally set to the pin's progressType, which is the only pin identifier the game sends
    /// </summary>
    public long PinId { get; set; }
    public int Progress { get; set; }
    [ForeignKey(nameof(PublisherId))]
    public GameUser Publisher { get; set; }
    
    public ObjectId PublisherId { get; set; }

    public DateTimeOffset FirstPublished { get; set; }
    public DateTimeOffset LastUpdated { get; set; }

    /// <summary>
    /// Whether this pin was achieved in retail LBP2/3/Vita or in a Beta build, to track the progress
    /// for these game groups seperately.
    /// </summary>
    public bool IsBeta { get; set; }
}