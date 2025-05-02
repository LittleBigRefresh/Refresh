using Refresh.GameServer.Types;

namespace Refresh.Database.Query;

public interface IEditUserPrivacySettings
{
    Visibility? LevelVisibility { get; set; }
    Visibility? ProfileVisibility { get; set; }
}