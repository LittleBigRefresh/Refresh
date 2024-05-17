using System.Reflection;
using JetBrains.Annotations;
using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Users
{
    private static readonly GameUser DeletedUser = new()
    {
        Location = GameLocation.Zero,
        Username = "!DeletedUser",
        Description = "I'm a fake user that represents deleted users for levels.",
        FakeUser = true,
    };
    
    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameUser? GetUserByUsername(string? username, bool caseSensitive = true)
    {
        if (username == null) return null;
        if (username == "!DeletedUser")
            return DeletedUser;
        if (username.StartsWith("!"))
            return new()
            {
                Location = GameLocation.Zero,
                Username = username,
                Description = "I'm a fake user that represents a non existent publisher for re-published levels.",
                FakeUser = true,
            };
        
        if (!caseSensitive)
            return this._realm.All<GameUser>().FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        
        return this._realm.All<GameUser>().FirstOrDefault(u => u.Username == username);
    }
    
    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameUser? GetUserByEmailAddress(string? emailAddress)
    {
        if (emailAddress == null) return null;
        emailAddress = emailAddress.ToLowerInvariant();
        return this._realm.All<GameUser>().FirstOrDefault(u => u.EmailAddress == emailAddress);
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

    public DatabaseList<GameUser> GetUsers(int count, int skip)
        => new(this._realm.All<GameUser>().OrderByDescending(u => u.JoinDate), skip, count);

    public void UpdateUserData(GameUser user, SerializedUpdateData data, TokenGame game)
    {
        this._realm.Write(() =>
        {
            if (data.Description != null)
                user.Description = data.Description;

            if (data.Location != null)
                user.Location = data.Location;

            if (data.PlanetsHash != null)
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (game)
                {
                    case TokenGame.LittleBigPlanet2:
                        user.Lbp2PlanetsHash = data.PlanetsHash;
                        user.Lbp3PlanetsHash = data.PlanetsHash;
                        break;
                    case TokenGame.LittleBigPlanet3:
                        user.Lbp3PlanetsHash = data.PlanetsHash;
                        break;
                    case TokenGame.LittleBigPlanetVita:
                        user.VitaPlanetsHash = data.PlanetsHash;
                        break;
                    case TokenGame.BetaBuild:
                        user.BetaPlanetsHash = data.PlanetsHash;
                        break;
                }

            // ReSharper disable once InvertIf
            if (data.IconHash != null)
                switch (game)
                {

                    case TokenGame.LittleBigPlanet1:
                    case TokenGame.LittleBigPlanet2:
                    case TokenGame.LittleBigPlanet3:
#if false // TODO: Enable this code once https://github.com/LittleBigRefresh/Refresh/issues/309 is resolved
                        //If the icon is a remote asset, then it will work on Vita as well, so set the Vita hash 
                        if (!data.IconHash.StartsWith('g'))
                        {
                            user.VitaIconHash = data.IconHash;
                        }
#endif
                        user.IconHash = data.IconHash;
                        break;
                    case TokenGame.LittleBigPlanetVita:
#if false // TODO: Enable this code once https://github.com/LittleBigRefresh/Refresh/issues/309 is resolved
                        //If the icon is a remote asset, then it will work on PS3 as well, so set the PS3 hash to it as well
                        if (!data.IconHash.StartsWith('g'))
                        {
                            user.IconHash = data.IconHash;
                        }
#endif
                        user.VitaIconHash = data.IconHash;
                        
                        break;
                    case TokenGame.LittleBigPlanetPSP:
                        //PSP icons are special and use a GUID system separate from the mainline games,
                        //so we separate PSP icons to another field
                        user.PspIconHash = data.IconHash;
                        break;
                    case TokenGame.BetaBuild:
                        user.BetaIconHash = data.IconHash;
                        break;
                }
        });
    }
    
    public void UpdateUserData(GameUser user, ApiUpdateUserRequest data)
    {
        this._realm.Write(() =>
        {
            if (data.EmailAddress != null && data.EmailAddress != user.EmailAddress)
            {
                user.EmailAddressVerified = false;
            }

            data.EmailAddress = data.EmailAddress?.ToLowerInvariant();

            if (data.IconHash != null)
                user.IconHash = data.IconHash;

            if (data.Description != null)
                user.Description = data.Description;

            if (data.AllowIpAuthentication != null)
                user.AllowIpAuthentication = data.AllowIpAuthentication.Value;

            if (data.PsnAuthenticationAllowed != null)
                user.PsnAuthenticationAllowed = data.PsnAuthenticationAllowed.Value;

            if (data.RpcnAuthenticationAllowed != null)
                user.RpcnAuthenticationAllowed = data.RpcnAuthenticationAllowed.Value;

            if (data.RedirectGriefReportsToPhotos != null)
                user.RedirectGriefReportsToPhotos = data.RedirectGriefReportsToPhotos.Value;
            
            if (data.UnescapeXmlSequences != null)
                user.UnescapeXmlSequences = data.UnescapeXmlSequences.Value;

            if (data.EmailAddress != null)
                user.EmailAddress = data.EmailAddress;
        });
    }

    [Pure]
    public int GetTotalUserCount() => this._realm.All<GameUser>().Count();
    
    [Pure]
    public int GetActiveUserCount()
    {
        DateTimeOffset lastMonth = this._time.Now.Subtract(TimeSpan.FromDays(30));
        return this._realm.All<GameUser>().Count(u => u.LastLoginDate > lastMonth);
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

    public void RenameUser(GameUser user, string newUsername)
    {
        string oldUsername = user.Username;
        
        this._realm.Write(() =>
        {
            user.Username = newUsername;
        });
        
        this.AddNotification("Username Updated", $"An admin has updated your account's username from '{oldUsername}' to '{newUsername}'. " +
                                                 $"If there are any problems caused by this, please let us know.", user);
        
        // Since ticket authentication is username-based, delete all game tokens for this user
        // Future authentication is going to be invalid, and the client is going to be in a broken state anyways.
        this.RevokeAllTokensForUser(user, TokenType.Game);
    }

    public void DeleteUser(GameUser user)
    {
        const string deletedReason = "This user's account has been deleted.";
        
        this.BanUser(user, deletedReason, DateTimeOffset.MaxValue);
        this.RevokeAllTokensForUser(user);
        this.DeleteNotificationsByUser(user);
        
        this._realm.Write(() =>
        {
            user.Pins = new UserPins();
            user.Location = new GameLocation();
            user.Description = deletedReason;
            user.EmailAddress = null;
            user.PasswordBcrypt = "deleted";
            user.JoinDate = DateTimeOffset.MinValue;
            user.LastLoginDate = DateTimeOffset.MinValue;
            user.Lbp2PlanetsHash = "0";
            user.Lbp3PlanetsHash = "0";
            user.VitaPlanetsHash = "0";
            user.IconHash = "0";
            user.AllowIpAuthentication = false;
            user.EmailAddressVerified = false;
            user.CurrentVerifiedIp = null;
            user.PsnAuthenticationAllowed = false;
            user.RpcnAuthenticationAllowed = false;

            // TODO: unit tests for this
            foreach (GamePhoto photo in this.GetPhotosWithUser(user, int.MaxValue, 0).Items)
                foreach (GamePhotoSubject subject in photo.Subjects.Where(s => s.User?.UserId == user.UserId))
                    subject.User = null;
            
            this._realm.RemoveRange(this._realm.All<FavouriteLevelRelation>().Where(r => r.User == user));
            this._realm.RemoveRange(this._realm.All<FavouriteUserRelation>().Where(r => r.UserToFavourite == user));
            this._realm.RemoveRange(this._realm.All<FavouriteUserRelation>().Where(r => r.UserFavouriting == user));
            this._realm.RemoveRange(this._realm.All<QueueLevelRelation>().Where(r => r.User == user));
            this._realm.RemoveRange(this._realm.All<GamePhoto>().Where(p => p.Publisher == user));

            foreach (GameLevel level in this._realm.All<GameLevel>().Where(l => l.Publisher == user))
            {
                level.Publisher = null;
            }
        });
    }

    public void ResetUserPlanets(GameUser user)
    {
        this._realm.Write(() =>
        {
            user.Lbp2PlanetsHash = "0";
            user.Lbp3PlanetsHash = "0";
            user.VitaPlanetsHash = "0";
        });
    }

    public void SetUnescapeXmlSequences(GameUser user, bool value)
    {
        this._realm.Write(() =>
        {
            user.UnescapeXmlSequences = value;
        });
    }
    
    public void SetUserGriefReportRedirection(GameUser user, bool value)
    {
        this._realm.Write(() =>
        {
            user.RedirectGriefReportsToPhotos = value;
        });
    }

    public void ClearForceMatch(GameUser user)
    {
        this._realm.Write(() =>
        {
            user.ForceMatch = null;
        });
    }

    public void SetForceMatch(GameUser user, GameUser target)
    {
        this._realm.Write(() =>
        {
            user.ForceMatch = target.ForceMatch;
        });
    }
    
    public void ForceUserTokenGame(Token token, TokenGame game)
    {
        this._realm.Write(() =>
        {
            token.TokenGame = game;
        });
    }
    
    public void ForceUserTokenPlatform(Token token, TokenPlatform platform)
    {
        this._realm.Write(() =>
        {
            token.TokenPlatform = platform;
        });
    }
    
    public void IncrementUserFilesizeQuota(GameUser user, int amount)
    {
        this._realm.Write(() =>
        {
            user.FilesizeQuotaUsage += amount;
        });
    }
}