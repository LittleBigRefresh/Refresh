using Bunkum.Core.Storage;
using JetBrains.Annotations;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

/// <summary>
/// A user with full information, like current role, ban status, etc.
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiExtendedGameUserResponse : IApiResponse, IDataConvertableFrom<ApiExtendedGameUserResponse, GameUser>
{
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public required string IconHash { get; set; }
    public required string Description { get; set; }
    public required ApiGameLocationResponse Location { get; set; }
    public required DateTimeOffset JoinDate { get; set; }
    public required DateTimeOffset LastLoginDate { get; set; }
    
    public required bool AllowIpAuthentication { get; set; }
    public required GameUserRole Role { get; set; }
    
    public required string? BanReason { get; set; }
    public required DateTimeOffset? BanExpiryDate { get; set; }
    
    public required bool RpcnAuthenticationAllowed { get; set; }
    public required bool PsnAuthenticationAllowed { get; set; }
    
    public bool RedirectGriefReportsToPhotos { get; set; } 
    public bool UnescapeXmlSequences { get; set; } 
    
    public required string? EmailAddress { get; set; }
    public required bool EmailAddressVerified { get; set; }
    public required bool ShouldResetPassword { get; set; }
    
    [ContractAnnotation("null => null; notnull => notnull")]
    public static ApiExtendedGameUserResponse? FromOld(GameUser? user)
    {
        if (user == null) return null;
        
        return new ApiExtendedGameUserResponse
        {
            UserId = user.UserId.ToString()!,
            Username = user.Username,
            IconHash = user.IconHash,
            Description = user.Description,
            Location = ApiGameLocationResponse.FromGameLocation(user.Location)!,
            JoinDate = user.JoinDate,
            LastLoginDate = user.LastLoginDate,
            AllowIpAuthentication = user.AllowIpAuthentication,
            Role = user.Role,
            BanReason = user.BanReason,
            BanExpiryDate = user.BanExpiryDate,
            RpcnAuthenticationAllowed = user.RpcnAuthenticationAllowed,
            PsnAuthenticationAllowed = user.PsnAuthenticationAllowed,
            EmailAddress = user.EmailAddress,
            EmailAddressVerified = user.EmailAddressVerified,
            ShouldResetPassword = user.ShouldResetPassword,
            RedirectGriefReportsToPhotos = user.RedirectGriefReportsToPhotos,
            UnescapeXmlSequences = user.UnescapeXmlSequences,
        };
    }
    
    public void FillInExtraData(GameDatabaseContext database, IDataStore dataStore)
    {
        this.IconHash = database.GetAssetFromHash(this.IconHash)?.GetAsIcon(TokenGame.Website, database, dataStore) ?? this.IconHash;
    }
    
    public static ApiExtendedGameUserResponse? FromOldWithExtraData(GameUser? old, GameDatabaseContext database, IDataStore dataStore)
    {
        if (old == null) return null;

        ApiExtendedGameUserResponse response = FromOld(old)!;
        response.FillInExtraData(database, dataStore);

        return response;
    }

    public static IEnumerable<ApiExtendedGameUserResponse> FromOldList(IEnumerable<GameUser> oldList) => oldList.Select(FromOld).ToList()!;
    
    public static IEnumerable<ApiExtendedGameUserResponse> FromOldListWithExtraData(IEnumerable<GameUser> oldList, GameDatabaseContext database, IDataStore dataStore) => oldList.Select(old => FromOldWithExtraData(old, database, dataStore)).ToList()!;
}