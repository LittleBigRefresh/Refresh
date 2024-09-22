namespace Refresh.GameServer.Types.OAuth.GitHub;

#nullable disable

public class GitHubApiUserResponse
{
    [JsonProperty("avatar_url")] public Uri AvatarUrl { get; set; }

    [JsonProperty("bio")] public string Bio { get; set; }

    [JsonProperty("blog")] public string Blog { get; set; }

    [JsonProperty("business_plus", NullValueHandling = NullValueHandling.Ignore)]
    public bool? BusinessPlus { get; set; }

    [JsonProperty("collaborators", NullValueHandling = NullValueHandling.Ignore)]
    public long? Collaborators { get; set; }

    [JsonProperty("company")] public string Company { get; set; }

    [JsonProperty("created_at")] public DateTimeOffset CreatedAt { get; set; }

    [JsonProperty("disk_usage", NullValueHandling = NullValueHandling.Ignore)]
    public long? DiskUsage { get; set; }

    [JsonProperty("email")] public string Email { get; set; }

    [JsonProperty("events_url")] public string EventsUrl { get; set; }

    [JsonProperty("followers")] public long Followers { get; set; }

    [JsonProperty("followers_url")] public Uri FollowersUrl { get; set; }

    [JsonProperty("following")] public long Following { get; set; }

    [JsonProperty("following_url")] public string FollowingUrl { get; set; }

    [JsonProperty("gists_url")] public string GistsUrl { get; set; }

    [JsonProperty("gravatar_id")] public string GravatarId { get; set; }

    [JsonProperty("hireable")] public bool? Hireable { get; set; }

    [JsonProperty("html_url")] public Uri HtmlUrl { get; set; }

    [JsonProperty("id")] public long Id { get; set; }

    [JsonProperty("ldap_dn", NullValueHandling = NullValueHandling.Ignore)]
    public string LdapDn { get; set; }

    [JsonProperty("location")] public string Location { get; set; }

    [JsonProperty("login")] public string Login { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("node_id")] public string NodeId { get; set; }

    [JsonProperty("notification_email")] public string NotificationEmail { get; set; }

    [JsonProperty("organizations_url")] public Uri OrganizationsUrl { get; set; }

    [JsonProperty("owned_private_repos", NullValueHandling = NullValueHandling.Ignore)]
    public long? OwnedPrivateRepos { get; set; }

    [JsonProperty("plan", NullValueHandling = NullValueHandling.Ignore)]
    public GitHubApiPlanResponse Plan { get; set; }

    [JsonProperty("private_gists", NullValueHandling = NullValueHandling.Ignore)]
    public long? PrivateGists { get; set; }

    [JsonProperty("public_gists")] public long PublicGists { get; set; }

    [JsonProperty("public_repos")] public long PublicRepos { get; set; }

    [JsonProperty("received_events_url")] public Uri ReceivedEventsUrl { get; set; }

    [JsonProperty("repos_url")] public Uri ReposUrl { get; set; }

    [JsonProperty("site_admin")] public bool SiteAdmin { get; set; }

    [JsonProperty("starred_url")] public string StarredUrl { get; set; }

    [JsonProperty("subscriptions_url")] public Uri SubscriptionsUrl { get; set; }

    [JsonProperty("suspended_at")] public DateTimeOffset? SuspendedAt { get; set; }

    [JsonProperty("total_private_repos", NullValueHandling = NullValueHandling.Ignore)]
    public long? TotalPrivateRepos { get; set; }

    [JsonProperty("twitter_username")] public string TwitterUsername { get; set; }

    [JsonProperty("two_factor_authentication", NullValueHandling = NullValueHandling.Ignore)]
    public bool? TwoFactorAuthentication { get; set; }

    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

    [JsonProperty("url")] public Uri Url { get; set; }

    public class GitHubApiPlanResponse
    {
        [JsonProperty("collaborators")] public long Collaborators { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("private_repos")] public long PrivateRepos { get; set; }

        [JsonProperty("space")] public long Space { get; set; }
    }
}