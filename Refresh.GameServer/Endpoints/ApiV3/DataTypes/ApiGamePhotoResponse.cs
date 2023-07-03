using Refresh.GameServer.Types.Photos;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes;

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
    
    public static ApiGamePhotoResponse? FromOld(GamePhoto? old)
    {
        if (old == null) return null;

        return new ApiGamePhotoResponse
        {
            PhotoId = old.PhotoId,
            TakenAt = old.TakenAt,
            PublishedAt = old.PublishedAt,

            Publisher = ApiGameUserResponse.FromOld(old.Publisher)!,
            Level = ApiGameLevelResponse.FromOld(old.Level),

            LevelName = old.LevelName,
            LevelType = old.LevelType,
            LevelId = old.LevelId,

            SmallHash = old.SmallHash,
            MediumHash = old.MediumHash,
            LargeHash = old.LargeHash,
            PlanHash = old.PlanHash,

            Subjects = ApiGamePhotoSubjectResponse.FromOldList(old.Subjects),
        };
    }

    public static IEnumerable<ApiGamePhotoResponse> FromOldList(IEnumerable<GamePhoto> oldList) => oldList.Select(FromOld)!;
}