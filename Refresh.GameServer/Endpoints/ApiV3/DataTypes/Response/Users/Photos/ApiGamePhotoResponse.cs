using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Levels;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Photos;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users.Photos;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGamePhotoResponse : IApiResponse, IDataConvertableFrom<ApiGamePhotoResponse, GamePhoto>
{
    public required int PhotoId { get; set; }
    public required DateTimeOffset TakenAt { get; set; }
    public required DateTimeOffset PublishedAt { get; set; }
    
    public required ApiGameUserResponse Publisher { get; set; }
    public required ApiGameLevelResponse? Level { get; set; }
    
    public required string LevelName { get; set; }
    public required string LevelType { get; set; }
    public required int LevelId { get; set; }
    
    public required string SmallHash { get; set; }
    public required string MediumHash { get; set; }
    public required string LargeHash { get; set; }
    public required string PlanHash { get; set; }
    
    public required IEnumerable<ApiGamePhotoSubjectResponse> Subjects { get; set; }
    
    public static ApiGamePhotoResponse? FromOld(GamePhoto? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiGamePhotoResponse
        {
            PhotoId = old.PhotoId,
            TakenAt = old.TakenAt,
            PublishedAt = old.PublishedAt,

            Publisher = ApiGameUserResponse.FromOld(old.Publisher, dataContext)!,
            Level = ApiGameLevelResponse.FromOld(old.Level, dataContext),

            LevelName = old.LevelName,
            LevelType = old.LevelType,
            LevelId = old.LevelId,

            SmallHash = old.SmallAsset.IsPSP ? $"psp/{old.SmallAsset.AssetHash}" : old.SmallAsset.AssetHash,
            MediumHash = old.MediumAsset.IsPSP ? $"psp/{old.MediumAsset.AssetHash}" : old.MediumAsset.AssetHash,
            LargeHash = old.LargeAsset.IsPSP ? $"psp/{old.LargeAsset.AssetHash}" : old.LargeAsset.AssetHash,
            PlanHash = old.PlanHash,

            Subjects = ApiGamePhotoSubjectResponse.FromOldList(old.Subjects, dataContext),
        };
    }

    public static IEnumerable<ApiGamePhotoResponse> FromOldList(IEnumerable<GamePhoto> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}