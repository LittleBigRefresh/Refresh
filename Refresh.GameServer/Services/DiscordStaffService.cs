using Bunkum.Core.Services;
using Discord;
using Discord.Webhook;
using NotEnoughLogs;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Types.UserData;
using GameAsset = Refresh.GameServer.Types.Assets.GameAsset;

namespace Refresh.GameServer.Services;

public class DiscordStaffService : EndpointService
{
    private readonly DiscordWebhookClient? _client;
    private readonly IntegrationConfig _config;
    
    private readonly string _externalUrl;

    private const string NameSuffix = " (Staff)";
    
    private const string DefaultResultsDescription = "These are the results of the AI's best guess at deciphering the contents of the image. " +
                                                     "Take them with a grain of salt as the AI isn't perfect.";

    internal DiscordStaffService(Logger logger, GameServerConfig gameConfig, IntegrationConfig config) : base(logger)
    {
        this._config = config;
        this._externalUrl = gameConfig.WebExternalUrl;
        
        if(config.DiscordStaffWebhookEnabled)
            this._client = new DiscordWebhookClient(config.DiscordStaffWebhookUrl);
    }
    
    private string GetAssetUrl(string hash)
    {
        return $"{this._externalUrl}/api/v3/assets/{hash}/image";
    }
    
    private string GetAssetInfoUrl(string hash)
    {
        return $"{this._externalUrl}/api/v3/assets/{hash}";
    }

    private void PostMessage(string? message = null, IEnumerable<Embed>? embeds = null!)
    {
        if (this._client == null)
            return;

        embeds ??= [];
        
        ulong id = this._client.SendMessageAsync(embeds: embeds, 
            username: this._config.DiscordNickname + NameSuffix, avatarUrl: this._config.DiscordAvatarUrl).Result;
        
        this.Logger.LogInfo(RefreshContext.Discord, $"Posted webhook {id}: '{message}'");
    }

    public void PostPredictionResult(Dictionary<string, float> results, GameAsset asset)
    {
        GameUser author = asset.OriginalUploader!;

        EmbedBuilder builder = new EmbedBuilder()
            .WithAuthor($"Image posted by @{author.Username} (id: {author.UserId})", this.GetAssetUrl(author.IconHash))
            .WithDescription(DefaultResultsDescription)
            .WithUrl(this.GetAssetInfoUrl(asset.AssetHash))
            .WithTitle($"AI Analysis of `{asset.AssetHash}`");
        
        foreach ((string tag, float confidence) in results.OrderByDescending(r => r.Value).Take(25))
        {
            string tagFormatted = this._config.AipiBannedTags.Contains(tag) ? $"{tag} (flagged!)" : tag;
            string confidenceFormatted = confidence.ToString("0.00%");
            builder.AddField(tagFormatted, confidenceFormatted, true);
        }
        
        this.PostMessage($"Prediction result for {asset.AssetHash} ({author.Username}):", [builder.Build()]);
    }
}