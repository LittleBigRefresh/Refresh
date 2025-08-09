using JetBrains.Annotations;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Data;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users.Rooms;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

/// <summary>
/// A user with full information, like current role, ban status, etc.
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiExtendedGameUserResponse : ApiGameUserResponse, IApiResponse, IDataConvertableFrom<ApiExtendedGameUserResponse, GameUser>
{
    public required string? BanReason { get; set; }
    public required DateTimeOffset? BanExpiryDate { get; set; }
    
    public required bool RpcnAuthenticationAllowed { get; set; }
    public required bool PsnAuthenticationAllowed { get; set; }

    public required bool RedirectGriefReportsToPhotos { get; set; }
    public required bool UnescapeXmlSequences { get; set; } 
    
    public required string? EmailAddress { get; set; }
    public required bool EmailAddressVerified { get; set; }
    public required bool ShouldResetPassword { get; set; }
    public required bool AllowIpAuthentication { get; set; }
    
    public required bool ShowModdedContent { get; set; }
    public required bool ShowReuploadedContent { get; set; }
    
    public required Visibility LevelVisibility { get; set; }
    public required Visibility ProfileVisibility { get; set; }
    
    public required int FilesizeQuotaUsage { get; set; }
    public required bool ConnectedToPresenceServer { get; set; }
    
    [ContractAnnotation("user:null => null; user:notnull => notnull")]
    public static new ApiExtendedGameUserResponse? FromOld(GameUser? user, DataContext dataContext)
    {
        if (user == null) return null;
        
        return new ApiExtendedGameUserResponse
        {
            UserId = user.UserId.ToString()!,
            Username = user.Username,
            IconHash = dataContext.GetIconFromHash(user.IconHash),
            VitaIconHash = dataContext.GetIconFromHash(user.VitaIconHash),
            BetaIconHash = dataContext.GetIconFromHash(user.BetaIconHash),
            YayFaceHash = dataContext.GetIconFromHash(user.YayFaceHash),
            BooFaceHash = dataContext.GetIconFromHash(user.BooFaceHash),
            MehFaceHash = dataContext.GetIconFromHash(user.MehFaceHash),
            Description = user.Description,
            Location = ApiGameLocationResponse.FromLocation(user.LocationX, user.LocationY)!,
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
            FilesizeQuotaUsage = user.FilesizeQuotaUsage,
            Statistics = ApiGameUserStatisticsResponse.FromOld(user, dataContext)!,
            ActiveRoom = ApiGameRoomResponse.FromOld(dataContext.Match.RoomAccessor.GetRoomByUser(user), dataContext),
            LevelVisibility = user.LevelVisibility,
            ProfileVisibility = user.ProfileVisibility,
            ShowModdedContent = user.ShowModdedContent,
            ShowReuploadedContent = user.ShowReuploadedContent,
            ConnectedToPresenceServer = user.PresenceServerAuthToken != null,
        };
    }

    public static new IEnumerable<ApiExtendedGameUserResponse> FromOldList(IEnumerable<GameUser> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}