using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Realms;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
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
    
    private static readonly object IdLock = new();
    private void AddSequentialObjectToDatabase<T>(T obj) where T : IRealmObject, ISequentialId
    {
        lock (IdLock)
        {
            this._realm.Write(() =>
            {
                int newId = this._realm.All<T>().Count() + 1;

                obj.SequentialId = newId;
                this._realm.Add(obj);
            });
        }
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
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameUser? GetUser(string? username)
    {
        if (username == null) return null;
        return this._realm.All<GameUser>().FirstOrDefault(u => u.Username == username);
    }

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
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameUser? GetUserFromTokenData(string? tokenData)
    {
        if (tokenData == null) return null;
        return this._realm.All<Token>().FirstOrDefault(t => t.TokenData == tokenData)?.User;
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public void UpdateUserData(GameUser user, UpdateUserData data)
    {
        this._realm.Write(() =>
        {
            if (data.Description != null) user.Description = data.Description;
            if (data.Location != null) user.Location = data.Location;
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

    public bool AddLevel(GameLevel level)
    {
        if (level.Publisher == null) throw new ArgumentNullException(nameof(level.Publisher));

        this.AddSequentialObjectToDatabase(level);
        
        return true;
    }

    [Pure]
    public IEnumerable<GameLevel> GetLevelsByUser(GameUser user, int count, int skip) =>
        this._realm.All<GameLevel>()
            .Where(l => l.Publisher == user)
            .AsEnumerable()
            .Skip(skip)
            .Take(count);
    
    [Pure]
    public IEnumerable<GameLevel> GetNewestLevels(int count, int skip) =>
        this._realm.All<GameLevel>()
            .OrderBy(l => l.PublishDate)
            .AsEnumerable()
            .Skip(skip)
            .Take(count);

    [Pure]
    public int GetTotalLevelCount() => this._realm.All<GameLevel>().Count();

    [Pure]
    public GameLevel? GetLevelById(int id) => this._realm.All<GameLevel>().FirstOrDefault(l => l.LevelId == id);
}