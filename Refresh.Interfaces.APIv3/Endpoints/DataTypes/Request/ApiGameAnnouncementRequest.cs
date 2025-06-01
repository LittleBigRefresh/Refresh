namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

#nullable disable

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameAnnouncementRequest
{
    public string Title { get; set; }
    public string Text { get; set; }
}