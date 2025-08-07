using System.Diagnostics;
using System.Xml.Serialization;
using Refresh.Common.Constants;
using Refresh.Core.Types.Data;
using Refresh.Database.Models;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;
using Refresh.Interfaces.Game.Types.Levels;
using Refresh.Interfaces.Game.Types.Lists;
using Refresh.Interfaces.Game.Types.UserData;

namespace Refresh.Interfaces.Game.Endpoints.DataTypes.Response;

[XmlRoot("user")]
public class GameUserResponse : IDataConvertableFrom<GameUserResponse, GameUser>
{
    [XmlAttribute("type")] public string Type { get; set; } = "user";
    [XmlElement("biography")] public required string Description { get; set; }
    [XmlElement("location")] public required GameLocation Location { get; set; }
    [XmlElement("planets")] public required string PlanetsHash { get; set; }
    
    [XmlElement("yay2")] public required string YayFaceHash { get; set; }
    [XmlElement("boo2")] public required string BooFaceHash { get; set; }
    [XmlElement("meh2")] public required string MehFaceHash { get; set; }
    
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
    [XmlElement("rootPlaylist")] public string? RootPlaylistId { get; set; }
    [XmlElement("pins")] public List<long> ProfilePins { get; set; } = [];
    
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
        
        if(old.Statistics == null)
            dataContext.Database.RecalculateUserStatistics(old);
        
        Debug.Assert(old.Statistics != null);

        TokenGame game = dataContext.Game;

        GameUserResponse response = new()
        {
            Description = old.Description,
            Location = new GameLocation(old.LocationX, old.LocationY),
            PlanetsHash = "0",
            
            YayFaceHash = dataContext.GetIconFromHash(old.YayFaceHash),
            BooFaceHash = dataContext.GetIconFromHash(old.BooFaceHash),
            MehFaceHash = dataContext.GetIconFromHash(old.MehFaceHash),
            
            Handle = SerializedUserHandle.FromUser(old, dataContext),
            CommentCount = old.Statistics.CommentCount,
            CommentsEnabled = true,
            FavouriteLevelCount = old.Statistics.FavouriteLevelCount,
            FavouriteUserCount = old.Statistics.FavouriteUserCount,
            QueuedLevelCount = old.Statistics.QueueCount,
            HeartCount = old.Statistics.FavouriteCount,
            PhotosByMeCount = old.Statistics.PhotosByUserCount,
            PhotosWithMeCount = old.Statistics.PhotosWithUserCount,
            ReviewCount = old.Statistics.ReviewCount,
            
            EntitledSlots = UgcLimits.MaximumLevels,
            EntitledSlotsLBP2 = UgcLimits.MaximumLevels,
            EntitledSlotsLBP3 = UgcLimits.MaximumLevels,
            UsedSlots = 0,
            UsedSlotsLBP2 = 0,
            UsedSlotsLBP3 = 0,
            PurchasedSlotsLBP2 = 0,
            PurchasedSlotsLBP3 = 0,
        };
        
        if (old.FakeUser)
        {
            response.PlanetsHash = "0";
            response.Handle.IconHash = "0";
            
            return response;
        }

        // Fill out information only relevant in certain games
        if (game is TokenGame.LittleBigPlanet1 or TokenGame.BetaBuild)
        {
            response.RootPlaylistId = dataContext.Database.GetUserRootPlaylist(old)?.PlaylistId.ToString();
        }
        if (game is not TokenGame.LittleBigPlanet1 or TokenGame.LittleBigPlanetPSP)
        {
            response.ProfilePins = dataContext.Database.GetProfilePinsByUser(old, dataContext.Game, 0, 3)
                .Items.Select(p => p.PinId).ToList();
        }

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
                response.FreeSlotsLBP3 = UgcLimits.MaximumLevels - response.UsedSlotsLBP3;
                //Fill out LBP2/LBP1 levels
                goto case TokenGame.LittleBigPlanet2;
            }
            case TokenGame.LittleBigPlanet2: {
                //Match all LBP2 levels
                response.UsedSlotsLBP2 = dataContext.Database.GetTotalLevelsPublishedByUser(old, TokenGame.LittleBigPlanet2);
                response.FreeSlotsLBP2 = UgcLimits.MaximumLevels - response.UsedSlotsLBP2;
                //Fill out LBP1 levels
                goto case TokenGame.LittleBigPlanet1;
            }
            case TokenGame.LittleBigPlanetVita: { 
                //Match all LBP Vita levels
                response.UsedSlotsLBP2 = dataContext.Database.GetTotalLevelsPublishedByUser(old, TokenGame.LittleBigPlanetVita);
                response.FreeSlotsLBP2 = UgcLimits.MaximumLevels - response.UsedSlotsLBP2;

                //Apply Vita-specific icon hash
                response.Handle.IconHash = old.VitaIconHash;
                break;
            }
            case TokenGame.LittleBigPlanet1: {
                //Match all LBP1 levels
                response.UsedSlots = dataContext.Database.GetTotalLevelsPublishedByUser(old, TokenGame.LittleBigPlanet1);
                response.FreeSlots = UgcLimits.MaximumLevels - response.UsedSlots;
                break;
            }
            case TokenGame.LittleBigPlanetPSP: {
                //Match all LBP PSP levels
                response.UsedSlots = dataContext.Database.GetTotalLevelsPublishedByUser(old, TokenGame.LittleBigPlanetPSP);
                response.FreeSlots = UgcLimits.MaximumLevels - response.UsedSlots;
                
                // Apply PSP-specific icon hash
                response.Handle.IconHash = old.PspIconHash;

                //Fill out PSP favourite users
                List<GameUser> users = dataContext.Database.GetUsersFavouritedByUser(old, 0, 20).Items.ToList();
                response.FavouriteUsers = new SerializedMinimalFavouriteUserList(users.Select(u => SerializedUserHandle.FromUser(u, dataContext)).ToList(), users.Count);

                //Fill out PSP favourite levels
                List<GameMinimalLevelResponse> favouriteLevels = dataContext.Database
                    .GetLevelsFavouritedByUser(old, 20, 0, new ResultFilterSettings(dataContext.Game), dataContext.User)
                    .Items
                    .Where(l => l.GameVersion == TokenGame.LittleBigPlanetPSP)
                    .Select(l => GameMinimalLevelResponse.FromOld(l, dataContext)).ToList()!;
                response.FavouriteLevels = new SerializedMinimalFavouriteLevelList(new SerializedMinimalLevelList(favouriteLevels, favouriteLevels.Count, favouriteLevels.Count));
                break;
            }
            case TokenGame.BetaBuild:
            {
                // only beta levels
                response.UsedSlots = dataContext.Database.GetTotalLevelsPublishedByUser(old, TokenGame.BetaBuild);
                response.FreeSlots = UgcLimits.MaximumLevels - response.UsedSlotsLBP2;
                
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