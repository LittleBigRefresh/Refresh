using Discord;
using Discord.Webhook;
using Refresh.Database.Query;
using Refresh.GameServer.Configuration;
using Refresh.Database;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Levels;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users.Photos;
using Refresh.Database.Models.Activity;
using Refresh.GameServer.Types.Data;

namespace Refresh.GameServer.Workers;

public class DiscordIntegrationWorker : IWorker
{
    private readonly IntegrationConfig _config;
    private readonly string _externalUrl;
    private readonly DiscordWebhookClient _client;
    
    private bool _firstCycle = true;

    private long _lastTimestamp;
    private static long Now => DateTimeOffset.Now.ToUnixTimeMilliseconds();
    public int WorkInterval => this._config.DiscordWorkerFrequencySeconds * 1000; // 60 seconds by default

    public DiscordIntegrationWorker(IntegrationConfig config, GameServerConfig gameConfig)
    {
        this._config = config;
        this._externalUrl = gameConfig.WebExternalUrl;

        this._client = new DiscordWebhookClient(config.DiscordWebhookUrl);
    }

    private void DoFirstCycle()
    {
        this._firstCycle = false;
        this._lastTimestamp = Now;
    }

    private string GetAssetUrl(string hash)
    {
        return $"{this._externalUrl}/api/v3/assets/{hash}/image";
    }

    private Embed? GenerateEmbedFromEvent(Event @event, DataContext context)
    {
        EmbedBuilder embed = new();

        ApiGameLevelResponse? level = @event.StoredDataType == EventDataType.Level ? 
            ApiGameLevelResponse.FromOld(context.Database.GetLevelById(@event.StoredSequentialId!.Value), context)
            : null;
        ApiGameUserResponse? user = @event.StoredDataType == EventDataType.User ? 
            ApiGameUserResponse.FromOld(context.Database.GetUserByObjectId(@event.StoredObjectId), context)
            : null;
        ApiGameScoreResponse? score = @event.StoredDataType == EventDataType.Score ? 
            ApiGameScoreResponse.FromOld(context.Database.GetScoreByObjectId(@event.StoredObjectId), context)
            : null;
        ApiGamePhotoResponse? photo = @event.StoredDataType == EventDataType.Photo ? 
            ApiGamePhotoResponse.FromOld(context.Database.GetPhotoFromEvent(@event), context)
            : null;
        
        if (photo != null)
            level = photo.Level;

        if (score != null) level = score.Level;

        string levelTitle = string.IsNullOrWhiteSpace(level?.Title) ? "Unnamed Level" : level.Title;

        string? levelLink = level == null ? null : $"[{levelTitle}]({this._externalUrl}/level/{level.LevelId})";
        string? userLink = user == null ? null : $"[{user.Username}]({this._externalUrl}/u/{user.UserId})";

        string? description = @event.EventType switch
        {
            EventType.LevelUpload => $"uploaded the level {levelLink}",
            EventType.LevelFavourite => $"gave {levelLink} a heart",
            EventType.LevelUnfavourite => null,
            EventType.UserFavourite => $"hearted {userLink}",
            EventType.UserUnfavourite => null,
            EventType.LevelPlay => null,
            EventType.LevelTag => null,
            EventType.LevelTeamPick => $"team picked {levelLink}",
            EventType.LevelRate => null,
            EventType.LevelReview => null,
            EventType.LevelScore => $"got {score!.Score:N0} points on {levelLink}",
            EventType.UserFirstLogin => "logged in for the first time",
            EventType.PhotoUpload => $"uploaded a photo{(photo is { Level: not null } ? $" on {levelLink}" : "")}",
            _ => null,
        };

        if (description == null) return null;
        embed.WithDescription($"[{@event.User.Username}]({this._externalUrl}/u/{@event.User.UserId}) {description}");

        if (photo != null)
            embed.WithImageUrl(this.GetAssetUrl(photo.LargeHash));
        else if (level != null) 
            embed.WithThumbnailUrl(this.GetAssetUrl(level.IconHash));
        else if (user != null)
            embed.WithThumbnailUrl(this.GetAssetUrl(user.IconHash));
        
        embed.WithTimestamp(@event.Timestamp);
        embed.WithAuthor(@event.User.Username, this.GetAssetUrl(@event.User.IconHash), $"{this._externalUrl}/u/{@event.UserId}");

        return embed.Build();
    }

    public void DoWork(DataContext context)
    {
        if (this._firstCycle)
        {
            this.DoFirstCycle();
        }

        DatabaseList<Event> activity = context.Database.GetGlobalRecentActivity(new ActivityQueryParameters
        {
            Timestamp = Now,
            EndTimestamp = this._lastTimestamp,
            Count = 5,
        });
        
        if (!activity.Items.Any()) return;

        this._lastTimestamp = activity.Items
            .Select(e => e.Timestamp.ToUnixTimeMilliseconds())
            .Max() + 1;

        IEnumerable<Embed> embeds = activity.Items
            .Reverse() // events are descending
            .Select(e => this.GenerateEmbedFromEvent(e, context))
            .Where(e => e != null)
            .ToList()!;

        if (!embeds.Any()) return;
        
        ulong id = this._client.SendMessageAsync(embeds: embeds, 
            username: this._config.DiscordNickname, avatarUrl: this._config.DiscordAvatarUrl).Result;
        
        context.Logger.LogInfo(RefreshContext.Worker, $"Posted webhook containing {activity.Items.Count()} events with id {id}");
    }
}