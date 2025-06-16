using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
[PrimaryKey(nameof(PinId), nameof(PublisherId), nameof(IsBeta))]
#endif
public partial class PinProgressRelation : IRealmObject
{
    /// <summary>
    /// Normally set to the pin's progressType, which is the only pin identifier the game sends
    /// </summary>
    public long PinId { get; set; }
    public int Progress { get; set; }
    [ForeignKey(nameof(PublisherId))]
    #if POSTGRES
    [Required]
    #endif
    public GameUser Publisher { get; set; }
    
    #if POSTGRES
    [Required]
    #endif
    [Ignored] public ObjectId PublisherId { get; set; }

    public DateTimeOffset FirstPublished { get; set; }
    public DateTimeOffset LastUpdated { get; set; }

    /// <summary>
    /// Whether this pin was achieved in retail LBP2/3/Vita or in a Beta build, to track the progress
    /// for these game groups seperately.
    /// </summary>
    public bool IsBeta { get; set; }
}