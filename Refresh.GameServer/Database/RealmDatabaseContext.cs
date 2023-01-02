using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;
using Realms;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Comments;
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
        //NOTE: we dont dispose the realm here, because the same thread may use it again, so we just `Refresh()` it
        this._realm.Refresh();
    }
    
    private static readonly object IdLock = new();
    // ReSharper disable once SuggestBaseTypeForParameter
    private void AddSequentialObject<T>(T obj, IList<T>? list = null, Action? writtenCallback = null) where T : IRealmObject, ISequentialId
    {
        lock (IdLock)
        {
            this._realm.Write(() =>
            {
                int newId = this._realm.All<T>().Count() + 1;

                obj.SequentialId = newId;

                this._realm.Add(obj);
                if(list == null) writtenCallback?.Invoke();
            });
        }
        
        // Two writes are necessary here for some unexplainable reason
        // We've already set a SequentialId so we can be outside the lock at this stage
        if (list != null)
        {
            this._realm.Write(() =>
            {
                list.Add(obj);
                writtenCallback?.Invoke();
            });
        }
    }

    private void AddSequentialObject<T>(T obj, Action writtenCallback = null) where T : IRealmObject, ISequentialId 
        => this.AddSequentialObject(obj, null, writtenCallback);

    private static long GetTimestampSeconds() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    private static long GetTimestampMilliseconds() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    #region Users

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
            PropertyInfo[] userProps = typeof(GameUser).GetProperties();
            foreach (PropertyInfo prop in typeof(UpdateUserData).GetProperties())
            {
                object? value = prop.GetValue(data);
                if(value == null) continue;

                PropertyInfo? userProp = userProps.FirstOrDefault(p => p.Name == prop.Name);
                if (userProp == null) throw new ArgumentOutOfRangeException(prop.Name);
                
                userProp.SetValue(user, value);
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
    #endregion

    #region Levels

    public bool AddLevel(GameLevel level)
    {
        if (level.Publisher == null) throw new ArgumentNullException(nameof(level.Publisher));

        long timestamp = GetTimestampSeconds();
        this.AddSequentialObject(level, () =>
        {
            level.PublishDate = timestamp;
            level.UpdateDate = timestamp;
        });

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
    public (IEnumerable<GameLevel> list, int count) SearchForLevels(int count, int skip, string query)
    {
        string[] keywords = query.Split(' ');
        if (keywords.Length == 0) return (Array.Empty<GameLevel>(), 0);
        
        IQueryable<GameLevel> levels = this._realm.All<GameLevel>();
        
        foreach (string keyword in keywords)
        {
            if(string.IsNullOrWhiteSpace(keyword)) continue;

            levels = levels.Where(l =>
                // l.LevelId.ToString() == keyword ||
                l.Title.Like(keyword, false) ||
                l.Description.Like(keyword, false)
            );
        }

        return (levels.AsEnumerable().Skip(skip).Take(count), levels.Count());
    }

    [Pure]
    public int GetTotalLevelCount() => this._realm.All<GameLevel>().Count();

    [Pure]
    public GameLevel? GetLevelById(int id) => this._realm.All<GameLevel>().FirstOrDefault(l => l.LevelId == id);
    #endregion
    
    #region Comments
    public void PostCommentToProfile(GameUser profile, GameUser author, string content)
    {
        GameComment comment = new()
        {
            Author = author,
            Content = content,
            Timestamp = GetTimestampMilliseconds(),
        };
        
        this.AddSequentialObject(comment, profile.ProfileComments);
    }

    public IEnumerable<GameComment> GetProfileComments(GameUser profile, int count, int skip) =>
            profile.ProfileComments
            .AsEnumerable()
            .Skip(skip)
            .Take(count);

    public int GetTotalProfileComments(GameUser profile) => profile.ProfileComments.Count;
    #endregion
}