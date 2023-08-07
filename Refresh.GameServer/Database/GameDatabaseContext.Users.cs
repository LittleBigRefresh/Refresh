using System.Reflection;
using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Users
{
    public GameUser CreateUser(string username)
    {
        GameUser user = new()
        {
            Username = username,
            JoinDate = this._time.Now,
        };

        this._realm.Write(() =>
        {
            this._realm.Add(user);
        });
        return user;
    }
    
    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameUser? GetUserByUsername(string? username)
    {
        if (username == null) return null;
        return this._realm.All<GameUser>().FirstOrDefault(u => u.Username == username);
    }

    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameUser? GetUserByObjectId(ObjectId? id)
    {
        if (id == null) return null;
        return this._realm.All<GameUser>().FirstOrDefault(u => u.UserId == id);
    }
    
    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameUser? GetUserByUuid(string? uuid)
    {
        if (uuid == null) return null;
        if(!ObjectId.TryParse(uuid, out ObjectId objectId)) return null;
        return this._realm.All<GameUser>().FirstOrDefault(u => u.UserId == objectId);
    }
    
    /// <summary>
    /// ID lookup for legacy API (v1). Do not use for any other purpose.
    /// </summary>
    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameUser? GetUserByLegacyId(int? legacyId)
    {
        if (legacyId == null) return null;
        
        // THIS SUCKS
        return this._realm.All<GameUser>().ToList().FirstOrDefault(u => u.UserId.Timestamp == legacyId);
    }
    
    private void UpdateUserData<TUpdateData>(GameUser user, TUpdateData data)
    {
        this._realm.Write(() =>
        {
            PropertyInfo[] userProps = typeof(GameUser).GetProperties();
            foreach (PropertyInfo prop in typeof(TUpdateData).GetProperties())
            {
                object? value = prop.GetValue(data);
                if(value == null) continue;

                PropertyInfo? userProp = userProps.FirstOrDefault(p => p.Name == prop.Name);
                if (userProp == null) throw new ArgumentOutOfRangeException(prop.Name);
                
                userProp.SetValue(user, value);
            }
        });
    }
    
    public void UpdateUserData(GameUser user, SerializedUpdateData data) 
        => this.UpdateUserData<SerializedUpdateData>(user, data);
    
    public void UpdateUserData(GameUser user, ApiUpdateUserRequest data) 
        => this.UpdateUserData<ApiUpdateUserRequest>(user, data);

    [Pure]
    public int GetTotalUserCount() => this._realm.All<GameUser>().Count();

    public void UpdateUserPins(GameUser user, UserPins pinsUpdate) 
    {
        this._realm.Write(() => {
            user.Pins = new UserPins();

            foreach (long pinsAward in pinsUpdate.Awards) user.Pins.Awards.Add(pinsAward);
            foreach (long pinsAward in pinsUpdate.Progress) user.Pins.Progress.Add(pinsAward);
            foreach (long profilePins in pinsUpdate.ProfilePins) user.Pins.ProfilePins.Add(profilePins);
        });
    }

    public void SetUserRole(GameUser user, GameUserRole role)
    {
        if(role == GameUserRole.Banned) throw new InvalidOperationException($"Cannot ban a user with this method. Please use {nameof(this.BanUser)}().");
        this._realm.Write(() =>
        {
            if (user.Role is GameUserRole.Banned or GameUserRole.Restricted)
            {
                user.BanReason = null;
                user.BanExpiryDate = null;
            };
            
            user.Role = role;
        });
    }

    private void PunishUser(GameUser user, string reason, DateTimeOffset expiryDate, GameUserRole role)
    {
        this._realm.Write(() =>
        {
            user.Role = role;
            user.BanReason = reason;
            user.BanExpiryDate = expiryDate;
        });
    }

    public void BanUser(GameUser user, string reason, DateTimeOffset expiryDate)
    {
        this.PunishUser(user, reason, expiryDate, GameUserRole.Banned);
        this.RevokeAllTokensForUser(user);
    }

    public void RestrictUser(GameUser user, string reason, DateTimeOffset expiryDate) 
        => this.PunishUser(user, reason, expiryDate, GameUserRole.Restricted);

    private bool IsUserPunished(GameUser user, GameUserRole role)
    {
        if (user.Role != role) return false;
        if (user.BanExpiryDate == null) return false;
        
        return user.BanExpiryDate >= this._time.Now;
    }

    public bool IsUserBanned(GameUser user) => this.IsUserPunished(user, GameUserRole.Banned);
    public bool IsUserRestricted(GameUser user) => this.IsUserPunished(user, GameUserRole.Restricted);

    public DatabaseList<GameUser> GetAllUsersWithRole(GameUserRole role)
    {
        // for some stupid reason, we have to do the byte conversion here or realm won't work correctly.
        byte roleByte = (byte)role;
        
        return new DatabaseList<GameUser>(this._realm.All<GameUser>().Where(u => u._Role == roleByte));
    }
}