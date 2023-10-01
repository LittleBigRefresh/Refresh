using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Legacy;

#nullable disable

[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class LegacyGameLevel : IDataConvertableFrom<LegacyGameLevel, GameLevel>
{
    public required int SlotId { get; set; }
    public required int InternalSlotId { get; set; }
    public required byte Type { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string IconHash { get; set; }
    public required bool IsAdventure { get; set; }
    public required int CreatorId { get; set; }
    public required bool InitiallyLocked { get; set; }
    public required bool SubLevel { get; set; }
    public required bool Lbp1Only { get; set; }
    public required byte Shareable { get; set; }
    public required string AuthorLabels { get; set; }
    public required string[] LevelTags { get; set; }
    public required int MinimumPlayers { get; set; }
    public required int MaximumPlayers { get; set; }
    public required bool MoveRequired { get; set; }
    public required long FirstUploaded { get; set; }
    public required long LastUpdated { get; set; }
    public required bool TeamPick { get; set; }
    public required LegacyGameLocation Location { get; set; }
    public required int GameVersion { get; set; }
    public required int Plays { get; set; }
    public required int PlaysUnique { get; set; }
    public required int PlaysComplete { get; set; }
    public required bool CommentsEnabled { get;set; }
    public required int AverageStarRating { get; set; }
    public required string LevelType { get; set; }
    
    public static LegacyGameLevel FromOld(GameLevel old) => new()
    {
        SlotId = old.LevelId,
        InternalSlotId = old.LevelId,
        Name = old.Title,
        Description = old.Description,
        IconHash = old.IconHash,
        Location = LegacyGameLocation.FromGameLocation(old.Location),
        Type = 1,
        IsAdventure = false,
        CreatorId = old.Publisher?.UserId.Timestamp ?? 0,
        InitiallyLocked = false,
        SubLevel = false,
        Lbp1Only = false,
        Shareable = 0,
        AuthorLabels = "",
        LevelTags = Array.Empty<string>(),
        MinimumPlayers = old.MinPlayers,
        MaximumPlayers = old.MaxPlayers,
        MoveRequired = false,
        FirstUploaded = old.PublishDate,
        LastUpdated = old.UpdateDate,
        TeamPick = old.TeamPicked,
        GameVersion = 1,
        Plays = old.AllPlays.Count(),
        PlaysUnique = old.UniquePlays.Count(),
        PlaysComplete = old.Scores.Count(),
        CommentsEnabled = true,
        AverageStarRating = 0,
        LevelType = "",
    };

    public static IEnumerable<LegacyGameLevel> FromOldList(IEnumerable<GameLevel> oldList) => oldList.Select(FromOld)!;
}