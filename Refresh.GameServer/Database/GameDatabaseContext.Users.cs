using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.Common.Constants;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Types.Challenges.LbpHub;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Users
{
    [Pure]
    [ContractAnnotation("username:null => null; username:notnull => canbenull")]
    public GameUser? GetUserByUsername(string? username, bool caseSensitive = true)
    {
        if (username == null) 
            return null;
        
        // Try the first pass to get the user
        GameUser? user = caseSensitive
            ? this.GameUsers.FirstOrDefault(u => u.Username == username)
            : this.GameUsers.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        
        // If that failed and the username is the deleted user, then we need to create the backing deleted user
        if (username == SystemUsers.DeletedUserName && user == null)
        {
            this.Write(() =>
            {
                this.GameUsers.Add(user = new GameUser
                {
                    Username = SystemUsers.DeletedUserName,
                    Description = SystemUsers.DeletedUserDescription,
                    FakeUser = true,
                });
            });
        } 
        // If that failed and the username is a fake re-upload user, then we need to create the backing fake user
        else if (username.StartsWith(SystemUsers.SystemPrefix) && user == null)
        {
            this.Write(() =>
            {
                this.GameUsers.Add(user = new GameUser
                {
                    Username = username,
                    Description = SystemUsers.UnknownUserDescription,
                    FakeUser = true,
                });
            });
        }
        
        return user;
    }
    
    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameUser? GetUserByEmailAddress(string? emailAddress)
    {
        if (emailAddress == null) return null;
        emailAddress = emailAddress.ToLowerInvariant();
        return this.GameUsers.FirstOrDefault(u => u.EmailAddress == emailAddress);
    }

    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameUser? GetUserByObjectId(ObjectId? id)
    {
        if (id == null) return null;
        return this.GameUsers.FirstOrDefault(u => u.UserId == id);
    }
    
    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameUser? GetUserByUuid(string? uuid)
    {
        if (uuid == null) return null;
        if(!ObjectId.TryParse(uuid, out ObjectId objectId)) return null;
        return this.GameUsers.FirstOrDefault(u => u.UserId == objectId);
    }

    public DatabaseList<GameUser> GetUsers(int count, int skip)
        => new(this.GameUsers.OrderByDescending(u => u.JoinDate), skip, count);

    public void UpdateUserData(GameUser user, SerializedUpdateData data, TokenGame game)
    {
        this.Write(() =>
        {
            if (data.Description != null)
                user.Description = data.Description;

            if (data.Location != null)
            {
                user.LocationX = data.Location.X;
                user.LocationY = data.Location.Y;
            }

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
            
            if (data.YayFaceHash != null)
                user.YayFaceHash = data.YayFaceHash;
            if (data.BooFaceHash != null)
                user.BooFaceHash = data.BooFaceHash;
            if (data.MehFaceHash != null)
                user.MehFaceHash = data.MehFaceHash;
        });
    }
    
    public void UpdateUserData(GameUser user, ApiUpdateUserRequest data)
    {
        this.Write(() =>
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
            
            if (data.UnescapeXmlSequences != null)
                user.UnescapeXmlSequences = data.UnescapeXmlSequences.Value;

            if (data.EmailAddress != null)
                user.EmailAddress = data.EmailAddress;
            
            if (data.LevelVisibility != null)
                user.LevelVisibility = data.LevelVisibility.Value;
            
            if (data.ProfileVisibility != null)
                user.ProfileVisibility = data.ProfileVisibility.Value;

            if (data.ShowModdedContent != null)
                user.ShowModdedContent = data.ShowModdedContent.Value;
        });
    }

    [Pure]
    public int GetTotalUserCount() => this.GameUsers.Count();
    
    [Pure]
    public int GetActiveUserCount()
    {
        DateTimeOffset timeFrame = this._time.Now.Subtract(TimeSpan.FromDays(7));
        return this.GameUsers.Count(u => u.LastLoginDate > timeFrame);
    }

    public void UpdateUserPins(GameUser user, UserPins pinsUpdate) 
    {
        this.Write(() => {
            user.Pins = new UserPins();

            foreach (long pinsAward in pinsUpdate.Awards) user.Pins.Awards.Add(pinsAward);
            foreach (long pinsAward in pinsUpdate.Progress) user.Pins.Progress.Add(pinsAward);
            foreach (long profilePins in pinsUpdate.ProfilePins) user.Pins.ProfilePins.Add(profilePins);
        });
    }

    public void SetUserRole(GameUser user, GameUserRole role)
    {
        if(role == GameUserRole.Banned) throw new InvalidOperationException($"Cannot ban a user with this method. Please use {nameof(this.BanUser)}().");
        this.Write(() =>
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
        this.Write(() =>
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
        
        return new DatabaseList<GameUser>(this.GameUsers.Where(u => u._Role == roleByte));
    }

    public void RenameUser(GameUser user, string newUsername)
    {
        string oldUsername = user.Username;
        
        this.Write(() =>
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
        
        this.Write(() =>
        {
            user.Pins = new UserPins();
            user.LocationX = 0;
            user.LocationY = 0;
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
            user.PsnAuthenticationAllowed = false;
            user.RpcnAuthenticationAllowed = false;
            
            foreach (GamePhoto photo in this.GetPhotosWithUser(user, int.MaxValue, 0).Items)
                foreach (GamePhotoSubject subject in photo.Subjects.Where(s => s.User?.UserId == user.UserId))
                    subject.User = null;
            
            this.GameSubmittedScores.RemoveRange(s => s.Players[0] == user);
            this.FavouriteLevelRelations.RemoveRange(r => r.User == user);
            this.FavouriteUserRelations.RemoveRange(r => r.UserToFavourite == user);
            this.FavouriteUserRelations.RemoveRange(r => r.UserFavouriting == user);
            this.QueueLevelRelations.RemoveRange(r => r.User == user);
            this.GamePhotos.RemoveRange(p => p.Publisher == user);
            this.GameUserVerifiedIpRelations.RemoveRange(p => p.User == user);
            
            foreach (GameLevel level in this.GameLevels.Where(l => l.Publisher == user))
            {
                level.Publisher = null;
            }

            this.GameChallengeScores.RemoveRange(s => s.Publisher == user);

            foreach (GameChallenge challenge in this.GameChallenges.Where(c => c.Publisher == user))
            {
                challenge.Publisher = null;
            }
        });
    }

    public void FullyDeleteUser(GameUser user)
    {
        // do an initial cleanup of everything before deleting the row  
        this.DeleteUser(user);
        
        this.Write(() =>
        {
            this.GameUsers.Remove(user);
        });
    }

    public void ResetUserPlanets(GameUser user)
    {
        this.Write(() =>
        {
            user.Lbp2PlanetsHash = "0";
            user.Lbp3PlanetsHash = "0";
            user.VitaPlanetsHash = "0";
        });
    }

    public void SetUnescapeXmlSequences(GameUser user, bool value)
    {
        this.Write(() =>
        {
            user.UnescapeXmlSequences = value;
        });
    }

    public void SetShowModdedContent(GameUser user, bool value)
    {
        this.Write(() =>
        {
            user.ShowModdedContent = value;
        });
    }

    public void ClearForceMatch(GameUser user)
    {
        this.Write(() =>
        {
            user.ForceMatch = null;
        });
    }

    public void SetForceMatch(GameUser user, GameUser target)
    {
        this.Write(() =>
        {
            user.ForceMatch = target.ForceMatch;
        });
    }
    
    public void ForceUserTokenGame(Token token, TokenGame game)
    {
        this.Write(() =>
        {
            token.TokenGame = game;
        });
    }
    
    public void ForceUserTokenPlatform(Token token, TokenPlatform platform)
    {
        this.Write(() =>
        {
            token.TokenPlatform = platform;
        });
    }
    
    public void IncrementUserFilesizeQuota(GameUser user, int amount)
    {
        this.Write(() =>
        {
            user.FilesizeQuotaUsage += amount;
        });
    }
    
    public void SetPrivacySettings(GameUser user, SerializedPrivacySettings settings) 
    {
        this.Write(() =>
        {
            if(settings.LevelVisibility != null)
                user.LevelVisibility = settings.LevelVisibility.Value;
            
            if (settings.ProfileVisibility != null)
                user.ProfileVisibility = settings.ProfileVisibility.Value;
        });            
    }

    public void MarkAllReuploads(GameUser user)
    {
        IQueryable<GameLevel> levels = this.GameLevels.Where(l => l.Publisher == user);
            
        this.Write(() =>
        {
            foreach (GameLevel level in levels)
            {
                level.IsReUpload = true;
                // normally, we'd also set the original publisher when marking a reupload.
                // but since were doing this blindly, we shouldn't because the level might already be a reupload.
                // we'd be setting it to null here, which could be loss of information.
            }
        });
    }

    public void SetUserRootPlaylist(GameUser user, GamePlaylist playlist)
    {
        this.Write(() =>
        {
            user.RootPlaylist = playlist;
        });
    }

    public void SetUserPresenceAuthToken(GameUser user, string? token)
    {
        this.Write(() =>
        {
            user.PresenceServerAuthToken = token;
        });
    }
}