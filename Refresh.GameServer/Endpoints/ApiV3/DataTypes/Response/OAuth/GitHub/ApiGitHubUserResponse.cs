using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.OAuth.GitHub;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.OAuth.GitHub;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGitHubUserResponse : IApiResponse, IDataConvertableFrom<ApiGitHubUserResponse, GitHubApiUserResponse>
{
    public required string? Username { get; set; }
    public required string? Name { get; set; }
    public required string? ProfileUrl { get; set; }
    public required string? AvatarUrl { get; set; }
    
    public static ApiGitHubUserResponse? FromOld(GitHubApiUserResponse? old, DataContext dataContext)
    {
        if (old == null)
            return null;

        return new ApiGitHubUserResponse
        {
            Username = old.Login,
            Name = old.Name,
            ProfileUrl = old.HtmlUrl.ToString(),
            AvatarUrl = old.AvatarUrl.ToString(),
        };
    }

    public static IEnumerable<ApiGitHubUserResponse> FromOldList(IEnumerable<GitHubApiUserResponse> oldList, DataContext dataContext)
        => oldList.Select(d => FromOld(d, dataContext)!);
}