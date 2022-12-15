using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Database.Types;
using Refresh.GameServer.Endpoints;
using Refresh.HttpServer.Database;

namespace Refresh.GameServer.Database;

public class RealmDatabaseContext : IDatabaseContext
{
    private readonly Realm _realm;

    internal RealmDatabaseContext(Realm realm)
    {
        this._realm = realm;
    }

    public void Dispose()
    {
        this._realm.Refresh();
        this._realm.Dispose();
    }
    
    public GameUser CreateUser(string username)
    {
        GameUser user = new()
        {
            Username = username,
        };

        this._realm.Write(() =>
        {
            this._realm.Add(user);
        });
        return user;
    }

    [Pure]
    public GameUser? GetUser(ObjectId id) => this._realm.Find<GameUser>(id);
    [Pure]
    public GameUser? GetUser(string username) => this._realm.All<GameUser>().FirstOrDefault(u => u.Username == username);
    
    public Token GenerateTokenForUser(GameUser user)
    {
        Token token = new()
        {
            User = user,
            TokenData = Guid.NewGuid().ToString(),
        };

        this._realm.Write(() =>
        {
            this._realm.Add(token);
        });
        
        return token;
    }

    [Pure]
    public GameUser? GetUserFromTokenData(string? tokenData)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (tokenData == null) return null;
        
        return this._realm.All<Token>().FirstOrDefault(t => t.TokenData == tokenData)?.User;
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public void UpdateUserData(GameUser user, UpdateUserData data)
    {
        this._realm.Write(() =>
        {
            if (data.Description != null) user.Description = data.Description;
            if (data.Location != null)
            {
                data.Location.LocationId = user.Location.LocationId;
                user.Location = data.Location;
            }
        });
    }

    public void UpdateUserPins(GameUser user, UserPins pinsUpdate) 
    {
        this._realm.Write(() => {
            user.Pins = new UserPins();

            foreach (long pinsAward in pinsUpdate.Awards) user.Pins.Awards.Add(pinsAward);
            foreach (long pinsAward in pinsUpdate.Progress) user.Pins.Progress.Add(pinsAward);
            foreach (long profilePins in pinsUpdate.ProfilePins) user.Pins.ProfilePins.Add(profilePins);
        });
    }
}