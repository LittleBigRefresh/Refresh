using System.Xml.Serialization;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.DataTypes.Response;

[XmlRoot("user")]
public class GameUserResponse : IDataConvertableFrom<GameUserResponse, GameUser>
{
    public const int MaximumLevels = 9_999;
    
    [XmlAttribute("type")] public string Type { get; set; } = "user";
    [XmlElement("biography")] public required string Description { get; set; }
    [XmlElement("location")] public required GameLocation Location { get; set; }
    [XmlElement("planets")] public required string PlanetsHash { get; set; }
    
    [XmlElement("npHandle")] public required SerializedUserHandle Handle { get; set; }
    [XmlElement("commentCount")] public int CommentCount { get; set; }
    [XmlElement("commentsEnabled")] public bool CommentsEnabled { get; set; }
    [XmlElement("reviewCount")] public int ReviewCount { get; set; }
    [XmlElement("favouriteSlotCount")] public int FavouriteLevelCount { get; set; }
    [XmlElement("favouriteUserCount")] public int FavouriteUserCount { get; set; }
    [XmlElement("lolcatftwCount")] public int QueuedLevelCount { get; set; }
    [XmlElement("heartCount")] public int HeartCount { get; set; }
    [XmlElement("photosByMeCount")] public int PhotosByMeCount { get; set; }
    [XmlElement("photosWithMeCount")] public int PhotosWithMeCount { get; set; }
    
    [XmlElement("freeSlots")] public int FreeSlots { get; set; }
    [XmlElement("lbp2FreeSlots")] public int FreeSlotsLBP2 { get; set; }
    [XmlElement("lbp3FreeSlots")] public int FreeSlotsLBP3 { get; set; }
    [XmlElement("entitledSlots")] public int EntitledSlots { get; set; }
    [XmlElement("lbp2EntitledSlots")] public int EntitledSlotsLBP2 { get; set; }
    [XmlElement("lbp3EntitledSlots")] public int EntitledSlotsLBP3 { get; set; }
    [XmlElement("lbp1UsedSlots")] public int UsedSlots { get; set; }
    [XmlElement("lbp2UsedSlots")] public int UsedSlotsLBP2 { get; set; }
    [XmlElement("lbp3UsedSlots")] public int UsedSlotsLBP3 { get; set; }
    [XmlElement("lbp2PurchasedSlots")] public int PurchasedSlotsLBP2 { get; set; }
    [XmlElement("lbp3PurchasedSlots")] public int PurchasedSlotsLBP3 { get; set; }
    
    /// <summary>
    /// The levels the user has favourited, only used by LBP PSP
    /// </summary>
    [XmlElement("favouriteSlots")] public SerializedMinimalFavouriteLevelList? FavouriteLevels { get; set; }
    /// <summary>
    /// The users the user has favourited, only used by LBP PSP
    /// </summary>
    [XmlElement("favouriteUsers")] public SerializedMinimalFavouriteUserList? FavouriteUsers { get; set; }
    
    public static GameUserResponse? FromOld(GameUser? old, DataContext dataContext)
    {
        if (old == null) return null;

        GameUserResponse response = new()
        {
            Description = old.Description,
            Location = new GameLocation(old.LocationX, old.LocationY),
            PlanetsHash = "0",
            
            Handle = SerializedUserHandle.FromUser(old, dataContext),
            CommentCount = old.ProfileComments.Count,
            CommentsEnabled = true,
            FavouriteLevelCount = old.IsManaged ? dataContext.Database.GetTotalLevelsFavouritedByUser(old) : 0,
            FavouriteUserCount = old.IsManaged ? dataContext.Database.GetTotalUsersFavouritedByUser(old) : 0,
            QueuedLevelCount = old.IsManaged ? dataContext.Database.GetTotalLevelsQueuedByUser(old) : 0,
            HeartCount = old.IsManaged ? dataContext.Database.GetTotalUsersFavouritingUser(old) : 0,
            PhotosByMeCount = old.IsManaged ? dataContext.Database.GetTotalPhotosByUser(old) : 0,
            PhotosWithMeCount = old.IsManaged ? dataContext.Database.GetTotalPhotosWithUser(old) : 0,
            
            EntitledSlots = MaximumLevels,
            EntitledSlotsLBP2 = MaximumLevels,
            EntitledSlotsLBP3 = MaximumLevels,
            UsedSlots = 0,
            UsedSlotsLBP2 = 0,
            UsedSlotsLBP3 = 0,
            PurchasedSlotsLBP2 = 0,
            PurchasedSlotsLBP3 = 0,
        };
        
        if (!old.IsManaged)
        {
            response.PlanetsHash = "0";
            response.Handle.IconHash = "0";
            
            return response;
        }
        
        response.ReviewCount = dataContext.Database.GetTotalReviewsByUser(old);
        
        response.PlanetsHash = dataContext.Game switch
        {
            TokenGame.LittleBigPlanet1 => "0",
            TokenGame.LittleBigPlanet2 => old.Lbp2PlanetsHash,
            TokenGame.LittleBigPlanet3 => old.Lbp3PlanetsHash,
            TokenGame.LittleBigPlanetVita => old.VitaPlanetsHash,
            TokenGame.LittleBigPlanetPSP => "0",
            TokenGame.Website => "0",
            TokenGame.BetaBuild => old.BetaPlanetsHash,
            _ => throw new ArgumentOutOfRangeException(nameof(dataContext.Game), dataContext.Game, null),
        };

        // Fill out slot usage information
        switch (dataContext.Game)
        {
            case TokenGame.LittleBigPlanet3: {
                //Match all LBP3 levels
                response.UsedSlotsLBP3 = dataContext.Database.GetTotalLevelsPublishedByUser(old, TokenGame.LittleBigPlanet3);
                response.FreeSlotsLBP3 = MaximumLevels - response.UsedSlotsLBP3;
                //Fill out LBP2/LBP1 levels
                goto case TokenGame.LittleBigPlanet2;
            }
            case TokenGame.LittleBigPlanet2: {
                //Match all LBP2 levels
                response.UsedSlotsLBP2 = dataContext.Database.GetTotalLevelsPublishedByUser(old, TokenGame.LittleBigPlanet2);
                response.FreeSlotsLBP2 = MaximumLevels - response.UsedSlotsLBP2;
                //Fill out LBP1 levels
                goto case TokenGame.LittleBigPlanet1;
            }
            case TokenGame.LittleBigPlanetVita: { 
                //Match all LBP Vita levels
                response.UsedSlotsLBP2 = dataContext.Database.GetTotalLevelsPublishedByUser(old, TokenGame.LittleBigPlanetVita);
                response.FreeSlotsLBP2 = MaximumLevels - response.UsedSlotsLBP2;

                //Apply Vita-specific icon hash
                response.Handle.IconHash = old.VitaIconHash;
                break;
            }
            case TokenGame.LittleBigPlanet1: {
                //Match all LBP1 levels
                response.UsedSlots = dataContext.Database.GetTotalLevelsPublishedByUser(old, TokenGame.LittleBigPlanet1);
                response.FreeSlots = MaximumLevels - response.UsedSlots;
                break;
            }
            case TokenGame.LittleBigPlanetPSP: {
                //Match all LBP PSP levels
                response.UsedSlots = dataContext.Database.GetTotalLevelsPublishedByUser(old, TokenGame.LittleBigPlanetPSP);
                response.FreeSlots = MaximumLevels - response.UsedSlots;
                
                // Apply PSP-specific icon hash
                response.Handle.IconHash = old.PspIconHash;

                //Fill out PSP favourite users
                List<GameUser> users = dataContext.Database.GetUsersFavouritedByUser(old, 20, 0).ToList();
                response.FavouriteUsers = new SerializedMinimalFavouriteUserList(users.Select(u => SerializedUserHandle.FromUser(u, dataContext)).ToList(), users.Count);

                //Fill out PSP favourite levels
                List<GameMinimalLevelResponse> favouriteLevels = dataContext.Database
                    .GetLevelsFavouritedByUser(old, 20, 0, new LevelFilterSettings(dataContext.Game), dataContext.User)
                    .Items
                    .Where(l => l._GameVersion == (int)TokenGame.LittleBigPlanetPSP)
                    .Select(l => GameMinimalLevelResponse.FromOld(l, dataContext)).ToList()!;
                response.FavouriteLevels = new SerializedMinimalFavouriteLevelList(new SerializedMinimalLevelList(favouriteLevels, favouriteLevels.Count, favouriteLevels.Count));
                break;
            }
            case TokenGame.BetaBuild:
            {
                // only beta levels
                response.UsedSlots = dataContext.Database.GetTotalLevelsPublishedByUser(old, TokenGame.BetaBuild);
                response.FreeSlots = MaximumLevels - response.UsedSlotsLBP2;
                
                // use the same values for LBP3 and LBP2 since they're all shared under one count
                response.UsedSlotsLBP3 = response.UsedSlots;
                response.FreeSlotsLBP3 = response.FreeSlots;
                
                response.UsedSlotsLBP2 = response.UsedSlots;
                response.FreeSlotsLBP2 = response.FreeSlots;
                break;
            }
            case TokenGame.Website: break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataContext.Game), dataContext.Game, null);
        }

        return response;
    }

    public static IEnumerable<GameUserResponse> FromOldList(IEnumerable<GameUser> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}