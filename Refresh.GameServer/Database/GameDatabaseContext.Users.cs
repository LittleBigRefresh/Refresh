using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Users
{
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

    [SuppressMessage("ReSharper", "InvertIf")]
    public void UpdateUserData(GameUser user, SerializedUpdateData data)
    {
        this._realm.Write(() =>
        {
            PropertyInfo[] userProps = typeof(GameUser).GetProperties();
            foreach (PropertyInfo prop in typeof(SerializedUpdateData).GetProperties())
            {
                object? value = prop.GetValue(data);
                if(value == null) continue;

                PropertyInfo? userProp = userProps.FirstOrDefault(p => p.Name == prop.Name);
                if (userProp == null) throw new ArgumentOutOfRangeException(prop.Name);
                
                userProp.SetValue(user, value);
            }
        });
    }
    
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
}