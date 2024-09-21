using Refresh.Common.Helpers;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.OAuth;
using Refresh.GameServer.Types.OAuth.Discord;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // oauth
{
    public string CreateOAuthRequest(GameUser user, IDateTimeProvider timeProvider, OAuthProvider provider)
    {
        string state = CryptoHelper.GetRandomBase64String(128);

        this.Write(() =>
        {
            this.OAuthRequests.Add(new OAuthRequest
            {
                User = user,
                State = state,
                ExpiresAt = timeProvider.Now + TimeSpan.FromHours(1), // 1 hour expiry
                Provider = provider,
            });
        });

        return state;
    }

    /// <summary>
    /// Returns the OAuthProvider used in a request
    /// </summary>
    /// <param name="state">The OAuth request state</param>
    /// <returns>The provider, or null if no request was found with that state</returns>
    public OAuthProvider? OAuthGetProviderForRequest(string state) 
        => this.OAuthRequests.FirstOrDefault(d => d.State == state)?.Provider;

    public GameUser SaveOAuthToken(string state, OAuth2AccessTokenResponse tokenResponse, IDateTimeProvider timeProvider)
    {
        if (tokenResponse.RefreshToken == null) 
            throw new ArgumentException("Token response is missing refresh token!", nameof(tokenResponse));

        OAuthRequest request = this.OAuthRequests.First(d => d.State == state);
        GameUser user = request.User;
        
        this.Write(() =>
        {
            OAuthTokenRelation? relation = this.OAuthTokenRelations.FirstOrDefault(d => d.User == request.User);
            if (relation == null)
            {
                this.OAuthTokenRelations.Add(relation = new OAuthTokenRelation
                {
                    User = request.User,
                    Provider = request.Provider,
                });
            }

            this.UpdateOAuthToken(relation, tokenResponse, timeProvider);
            
            this.OAuthRequests.Remove(request);
        });

        return user;
    }

    public void UpdateOAuthToken(OAuthTokenRelation token, OAuth2AccessTokenResponse tokenResponse, IDateTimeProvider timeProvider)
    {
        this.Write(() =>
        {
            token.AccessToken = tokenResponse.AccessToken;
            token.RefreshToken = tokenResponse.RefreshToken;
            token.AccessTokenRevocationTime = timeProvider.Now + TimeSpan.FromSeconds(tokenResponse.ExpiresIn);
        });
    }

    public OAuthTokenRelation? GetOAuthTokenFromUser(GameUser user, OAuthProvider provider) 
        => this.OAuthTokenRelations.FirstOrDefault(d => d.User == user && d._Provider == (int)provider);

    public int RemoveAllExpiredOAuthRequests(IDateTimeProvider timeProvider)
    {
        IQueryable<OAuthRequest> expired = this.OAuthRequests.Where(d => d.ExpiresAt < timeProvider.Now);

        int removed = expired.Count();
        
        this.Write(() =>
        {
            this.OAuthRequests.RemoveRange(expired);
        });

        return removed;
    }

    public void RevokeOAuthToken(OAuthTokenRelation token)
    {
        this.Write(() =>
        {
            this.OAuthTokenRelations.Remove(token);
        });
    }
}