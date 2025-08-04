namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Data;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiRatingResponse : IApiResponse
{
    public required int YayRatings { get; set; }
    public required int BooRatings { get; set; }
    public required int OwnRating { get; set; }

    public static ApiRatingResponse FromRating(int yayRatings, int booRatings, int? ownRating)
    {
        return new()
        {
            YayRatings = yayRatings,
            BooRatings = booRatings,
            OwnRating = ownRating ?? 0,
        };
    }
}