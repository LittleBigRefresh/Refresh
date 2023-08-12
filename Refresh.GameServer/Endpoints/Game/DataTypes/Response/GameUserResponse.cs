using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.DataTypes.Response;

[XmlRoot("user")]
public class GameUserResponse : IDataConvertableFrom<GameUserResponse, GameUser>
{
    [XmlAttribute("type")] public string Type { get; set; } = "user";
    [XmlIgnore] public required string IconHash { get; set; }
    [XmlElement("biography")] public required string Description { get; set; }
    [XmlElement("location")] public required GameLocation Location { get; set; }
    [XmlElement("planets")] public required string PlanetsHash { get; set; }
    
    [XmlElement("npHandle")] public SerializedUserHandle Handle { get; set; }
    [XmlElement("commentCount")] public int CommentCount { get; set; }
    [XmlElement("commentsEnabled")] public bool CommentsEnabled { get; set; }
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
    
    public static GameUserResponse? FromOld(GameUser? old)
    {
        if (old == null) return null;

        GameUserResponse response = new()
        {
            IconHash = old.IconHash,
            Description = old.Description,
            Location = old.Location,
            PlanetsHash = old.PlanetsHash,
            
            Handle = SerializedUserHandle.FromUser(old),
            CommentCount = old.ProfileComments.Count,
            CommentsEnabled = true,
            FavouriteLevelCount = old.FavouriteLevelRelations.Count(),
            FavouriteUserCount = old.UsersFavourited.Count(),
            QueuedLevelCount = old.QueueLevelRelations.Count(),
            HeartCount = old.UsersFavouritingMe.Count(),
            PhotosByMeCount = old.PhotosByMe.Count(),
            PhotosWithMeCount = old.PhotosWithMe.Count(),
            
            EntitledSlots = 100,
            EntitledSlotsLBP2 = 100,
            EntitledSlotsLBP3 = 100,
            UsedSlots = 0,
            UsedSlotsLBP2 = old.PublishedLevels.Count(),
            UsedSlotsLBP3 = 0,
            PurchasedSlotsLBP2 = 0,
            PurchasedSlotsLBP3 = 0,
            FreeSlots = 100,
            FreeSlotsLBP3 = 100,
        };

        response.FreeSlotsLBP2 = 100 - response.UsedSlotsLBP2;

        return response;
    }

    public static IEnumerable<GameUserResponse> FromOldList(IEnumerable<GameUser> oldList) => oldList.Select(FromOld)!;
}