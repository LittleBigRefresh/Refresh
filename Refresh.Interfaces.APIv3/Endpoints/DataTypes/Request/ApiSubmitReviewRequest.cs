using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Database.Query;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiSubmitReviewRequest : ISubmitReviewRequest
{
    public RatingType? LevelRating { get; set; }
    public List<Label>? Labels { get; set; }
    public string? Content { get; set; }
}