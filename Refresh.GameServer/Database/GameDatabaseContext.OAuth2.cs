using Refresh.Common.Helpers;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.OAuth2.Discord;
using Refresh.GameServer.Types.OAuth2.Discord.Api;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // oauth2
{
    public string CreateDiscordOAuth2Request(GameUser user, IDateTimeProvider timeProvider)
    {
        string state = CryptoHelper.GetRandomBase64String(128);

        this.Write(() =>
        {
            this.DiscordOAuthRequests.Add(new DiscordOAuthRequest
            {
                UserId = user.UserId,
                State = state,
                ExpiresAt = timeProvider.Now + TimeSpan.FromDays(1), // 1 day expiry
            });
        });

        return state;
    }

    public bool DiscordOAuth2RequestExists(string state)
    {
        return this.DiscordOAuthRequests.Any(d => d.State == state);
    }

    public void AuthenticateDiscordOAuth2Request(string state, DiscordApiOAuth2AccessTokenResponse tokenResponse,
        IDateTimeProvider timeProvider)
    {
        DiscordOAuthRequest request = this.DiscordOAuthRequests.First(d => d.State == state);

        this.Write(() =>
        {
            DiscordOAuthTokenRelation? relation = this.DiscordOAuthRelations.FirstOrDefault(d => d.UserId == request.UserId);
            if (relation == null)
            {
                this.DiscordOAuthRelations.Add(relation = new DiscordOAuthTokenRelation
                {
                    UserId = request.UserId,
                });
            }

            this.UpdateDiscordOAuth2Token(relation, tokenResponse, timeProvider);
            
            this.DiscordOAuthRequests.Remove(request);
        });
    }

    public void UpdateDiscordOAuth2Token(DiscordOAuthTokenRelation token, DiscordApiOAuth2AccessTokenResponse tokenResponse, IDateTimeProvider timeProvider)
    {
        this.Write(() =>
        {
            token.AccessToken = tokenResponse.AccessToken;
            token.RefreshToken = tokenResponse.RefreshToken;
            token.AccessTokenRevocationTime = timeProvider.Now + TimeSpan.FromSeconds(tokenResponse.ExpiresIn);
        });
    }

    public DiscordOAuthTokenRelation? GetDiscordOAuth2TokenFromUser(GameUser user) 
        => this.DiscordOAuthRelations.FirstOrDefault(d => d.UserId == user.UserId);

    public void RemoveAllExpiredDiscordOAuth2Requests(IDateTimeProvider timeProvider)
    {
        this.Write(() =>
        {
            this.DiscordOAuthRequests.RemoveRange(d => d.ExpiresAt < timeProvider.Now);
        });
    }

    public void RemoveDiscordOAuth2Token(DiscordOAuthTokenRelation token)
    {
        this.Write(() =>
        {
            this.DiscordOAuthRelations.Remove(token);
        });
    }
}