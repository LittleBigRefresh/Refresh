namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

#nullable disable

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameAnnouncementRequest
{
    public string Title { get; set; }
    public string Text { get; set; }
}