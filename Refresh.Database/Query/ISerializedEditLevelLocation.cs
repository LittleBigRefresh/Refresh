using Refresh.GameServer.Types;

namespace Refresh.Database.Query;

public interface ISerializedEditLevelLocation
{
    string Type { get; set; }
    int LevelId { get; set; }
    GameLocation Location { get; set; }
}