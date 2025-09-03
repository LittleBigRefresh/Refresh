using Refresh.Database.Models;
using Refresh.Database.Models.Levels;

namespace Refresh.Database.Query;

public interface ISerializedPublishLevel
{
    string Title { get; set; }
    string Description { get; set; }
    string IconHash { get; set; }
    GameLocation Location { get; set; }
    IEnumerable<Label> FinalPublisherLabels { get; set; }
    string RootResource { get; set; }
    int MinPlayers { get; set; }
    int MaxPlayers { get; set; }
    bool EnforceMinMaxPlayers { get; set; }
    bool SameScreenGame { get; set; }
    string LevelType { get; set; }
    bool IsLocked { get; set; }
    bool IsSubLevel { get; set; }
    int IsCopyable { get; set; }
    bool RequiresMoveController { get; set; }
    bool IsAdventure { get; set; }
    string? BackgroundGuid { get; set; }
}