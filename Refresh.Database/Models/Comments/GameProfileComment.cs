using Refresh.Database.Models.Users;

#if POSTGRES
using PrimaryKeyAttribute = Refresh.Database.Compatibility.PrimaryKeyAttribute;
#endif

namespace Refresh.Database.Models.Comments;

#nullable disable

public partial class GameProfileComment : IRealmObject, IGameComment, ISequentialId
{
    [Key, PrimaryKey] public int SequentialId { get; set; }

    /// <inheritdoc/>
    #if POSTGRES
    [Required]
    #endif
    public GameUser Author { get; set; } = null!;

    /// <summary>
    /// The destination profile this comment was posted to.
    /// </summary>
    #if POSTGRES
    [Required]
    #endif
    public GameUser Profile { get; set; } = null!;
    
    /// <inheritdoc/>
    public string Content { get; set; } = string.Empty;
    
    /// <inheritdoc/>
    public DateTimeOffset Timestamp { get; set; } 
}