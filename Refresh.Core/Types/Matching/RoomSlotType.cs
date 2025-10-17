using Refresh.Database.Models.Levels;

namespace Refresh.Core.Types.Matching;

public enum RoomSlotType : byte
{
    Story = 0,
    Online = 1,
    Moon = 2,
    Pod = 5,
    DLC = 8,
    StoryAdventure = 11, // Used for both Story and DLC adventures
    OnlineAdventure = 12,
    MoonAdventure = 15,
}

public static class RoomSlotTypeExtensions
{
    public static GameSlotType ToGameSlotType(this RoomSlotType slotType)
    {
        switch (slotType)
        {
            case RoomSlotType.Story:
            case RoomSlotType.DLC:
            case RoomSlotType.StoryAdventure:
                return GameSlotType.Story;
            case RoomSlotType.Online:
            case RoomSlotType.OnlineAdventure:
                return GameSlotType.User;
            case RoomSlotType.Moon:
            case RoomSlotType.MoonAdventure:
                return GameSlotType.Moon;
            case RoomSlotType.Pod:
                return GameSlotType.Pod;
            default:
                throw new ArgumentOutOfRangeException(nameof(slotType), slotType, null);
        }
    }
}