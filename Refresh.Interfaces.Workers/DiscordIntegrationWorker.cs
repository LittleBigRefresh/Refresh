using Discord;
using Discord.Webhook;
using Refresh.Core;
using Refresh.Core.Configuration;
using Refresh.Database;
using Refresh.Database.Models.Activity;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers;

public class DiscordIntegrationJob : WorkerJob
{
    private readonly IntegrationConfig _config;
    private readonly string _externalUrl;
    private readonly DiscordWebhookClient _client;

    private long _lastTimestamp;
    private static long Now => DateTimeOffset.Now.ToUnixTimeMilliseconds();
    public override int WorkInterval => this._config.DiscordWorkerFrequencySeconds * 1000; // 60 seconds by default

    public DiscordIntegrationJob(IntegrationConfig config, GameServerConfig gameConfig)
    {
        this._config = config;
        this._externalUrl = gameConfig.WebExternalUrl;

        this._client = new DiscordWebhookClient(config.DiscordWebhookUrl);
    }

    private string GetAssetUrl(string hash)
    {
        return $"{this._externalUrl}/api/v3/assets/{hash}/image";
    }

    private Embed? GenerateEmbedFromEvent(Event @event, WorkContext context)
    {
        EmbedBuilder embed = new();

        GameLevel? level = @event.StoredDataType == EventDataType.Level ? 
            context.Database.GetLevelById(@event.StoredSequentialId!.Value)
            : null;
        GameUser? user = @event.StoredDataType == EventDataType.User ? 
            context.Database.GetUserByObjectId(@event.StoredObjectId)
            : null;
        GameScore? score = @event.StoredDataType == EventDataType.Score ? 
            context.Database.GetScoreByObjectId(@event.StoredObjectId)
            : null;
        GamePhoto? photo = @event.StoredDataType == EventDataType.Photo ? 
            context.Database.GetPhotoFromEvent(@event)
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
            embed.WithImageUrl(this.GetAssetUrl(photo.LargeAsset.AssetHash));
        else if (level != null) 
            embed.WithThumbnailUrl(this.GetAssetUrl(level.IconHash));
        else if (user != null)
            embed.WithThumbnailUrl(this.GetAssetUrl(user.IconHash));
        
        embed.WithTimestamp(@event.Timestamp);
        embed.WithAuthor(@event.User.Username, this.GetAssetUrl(@event.User.IconHash), $"{this._externalUrl}/u/{@event.User.UserId}");

        return embed.Build();
    }

    public override void ExecuteJob(WorkContext context)
    {
        if (this.FirstCycle)
            this._lastTimestamp = Now;

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