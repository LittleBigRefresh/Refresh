using System.Text;

namespace Refresh.Database.Models.Levels;

public enum Label : byte
{
    // Category 1
    Short = 0,
    Long = 1,
    Challenging = 2,
    Easy = 3,
    Scary = 4,
    Funny = 5,
    Artistic = 6,
    Musical = 7,
    Singleplayer = 8,
    Multiplayer = 9,
    RPG = 10,
    // Category 2
    Cinematic = 11,
    Versus = 12,
    Fighter = 13,
    Gallery = 14,
    Puzzler = 15,
    Platformer = 16,
    Racer = 17,
    Shooter = 18,
    Sports = 19,
    Story = 20,
    Strategy = 21,
    SurvivalChallenge = 22,
    Tutorial = 23,
    Retro = 24,
    TopDown = 25,
    Cooperative = 26,
    FirstPerson = 27,
    ThirdPerson = 28,
    SciFi = 29,
    Social = 30,
    ArcadeGame = 31,
    BoardGame = 32,
    CardGame = 33,
    MiniGame = 34,
    PartyGame = 35,
    Defence = 36,
    Driving = 37,
    Hangout = 38,
    HideAndSeek = 39,
    PropHunt = 40,
    MusicGallery = 41,
    CostumeGallery = 42,
    StickerGallery = 43,
    Movie = 44,
    Pinball = 45,
    Technology = 46,
    Homage = 47,
    EightBit = 48,
    SixteenBit = 49,
    Seasonal = 50,
    TimeTrial = 51,
    // Category 3
    Collectables = 52, 
    Controlinator = 53,
    Explosives = 54,
    Glitch = 55,
    GrapplingHook = 56,
    BouncePads = 57,
    Creatinator = 58,
    LowGravity = 59,
    Paintinator = 60,
    Grabinators = 61,
    Sackbots = 62,
    Vehicles = 63,
    Water = 64,
    BrainCrane = 65,
    Movinator = 66,
    Paint = 67,
    AttractGel = 68,
    AttractTweaker = 69,
    HeroCape = 70,
    Memoriser = 71,
    WallJump = 72,
    InteractiveStream = 73,
    Quests = 74,
    Sackpocket = 75,
    Springinator = 76,
    Hoverboard = 77,
    FloatyFluid = 78,
    // Category 4
    Oddsock = 79,
    Toggle = 80,
    Swoop = 81,
    Sackboy = 82,
    CreatedCharacters = 83,
    // LBP Vita exclusive
    Precision = 84,
    Flick = 85,
    RearTouch = 86,
    SharedScreen = 87,
    Swipe = 88,
    Tap = 89,
    Tilt = 90,
    Touch = 91,
    Portrait = 92,
    MultiLevel = 93,
    // not in LBP3
    Intricate = 94,
    // LBP3 duplicates
    SingleplayerLbp3 = 95,
    CooperativeLbp3 = 96,
}

public static class LabelExtensions
{
    static LabelExtensions()
    {
        // Create the conversion which goes the other way
        foreach ((string key, Label value) in LabelsMap)
        {
            StringMap[value] = key;
        }
    }
    
    private static readonly Dictionary<Label, string> StringMap = new();
    private static readonly Dictionary<string, Label> LabelsMap = new()
    {
        { "LABEL_Quick", Label.Short },
        { "LABEL_Long", Label.Long },
        { "LABEL_Challenging", Label.Challenging },
        { "LABEL_Easy", Label.Easy },
        { "LABEL_Scary", Label.Scary },
        { "LABEL_Funny", Label.Funny },
        { "LABEL_Challenging", Label.Challenging },
        { "LABEL_Artistic", Label.Artistic },
        { "LABEL_Musical", Label.Musical },
        { "LABEL_Intricate", Label.Intricate },

        { "LABEL_SinglePlayer", Label.Singleplayer },
        { "LABEL_SINGLE_PLAYER", Label.SingleplayerLbp3 },
        { "LABEL_Multiplayer", Label.Multiplayer },
        { "LABEL_RPG", Label.RPG },
        { "LABEL_Cinematic", Label.Cinematic },
        { "LABEL_Competitive", Label.Versus },
        { "LABEL_Fighter", Label.Fighter },
        { "LABEL_Gallery", Label.Gallery },
        { "LABEL_Puzzle", Label.Puzzler },
        { "LABEL_Platform", Label.Platformer },

        { "LABEL_Race", Label.Racer },
        { "LABEL_Shooter", Label.Shooter },
        { "LABEL_Sports", Label.Sports },
        { "LABEL_Story", Label.Story },
        { "LABEL_Strategy", Label.Strategy },
        { "LABEL_SurvivalChallenge", Label.SurvivalChallenge },
        { "LABEL_Tutorial", Label.Tutorial },
        { "LABEL_Retro", Label.Retro },
        { "LABEL_TOP_DOWN", Label.TopDown },
        { "LABEL_Co-op", Label.Cooperative },
        { "LABEL_CO_OP", Label.CooperativeLbp3 },
        { "LABEL_Precision", Label.Precision },

        { "LABEL_1st_Person", Label.FirstPerson },
        { "LABEL_3rd_Person", Label.ThirdPerson },
        { "LABEL_Sci_Fi", Label.SciFi },
        { "LABEL_Social", Label.Social },
        { "LABEL_Arcade_Game", Label.ArcadeGame },
        { "LABEL_Board_Game", Label.BoardGame },
        { "LABEL_Card_Game", Label.CardGame },
        { "LABEL_Mini_Game", Label.MiniGame },
        { "LABEL_Party_Game", Label.PartyGame },
        { "LABEL_Defence", Label.Defence },

        { "LABEL_Driving", Label.Driving },
        { "LABEL_Hangout", Label.Hangout },
        { "LABEL_Hide_And_Seek", Label.HideAndSeek },
        { "LABEL_Prop_Hunt", Label.PropHunt },
        { "LABEL_Music_Gallery", Label.MusicGallery },
        { "LABEL_Costume_Gallery", Label.CostumeGallery },
        { "LABEL_Sticker_Gallery", Label.StickerGallery },
        { "LABEL_Pinball", Label.Pinball },
        { "LABEL_Technology", Label.Technology },
        { "LABEL_Homage", Label.Homage },

        { "LABEL_8_Bit", Label.EightBit },
        { "LABEL_16_Bit", Label.SixteenBit },
        { "LABEL_Seasonal", Label.Seasonal },
        { "LABEL_Time_Trial", Label.TimeTrial },
        { "LABEL_Collectables", Label.Collectables },
        { "LABEL_DirectControl", Label.Controlinator },
        { "LABEL_Explosives", Label.Explosives },
        { "LABEL_Glitch", Label.Glitch },
        { "LABEL_GrapplingHook", Label.GrapplingHook },
        { "LABEL_JumpPads", Label.BouncePads },

        { "LABEL_MagicBag", Label.Creatinator },
        { "LABEL_LowGravity", Label.LowGravity },
        { "LABEL_Paintinator", Label.Paintinator },
        { "LABEL_PowerGlove", Label.Grabinators },
        { "LABEL_Sackbots", Label.Sackbots },
        { "LABEL_Vehicles", Label.Vehicles },
        { "LABEL_Water", Label.Water },
        { "LABEL_Brain_Crane", Label.BrainCrane },
        { "LABEL_Movinator", Label.Movinator },
        { "LABEL_Paint", Label.Paint },

        { "LABEL_ATTRACT_GEL", Label.AttractGel },
        { "LABEL_ATTRACT_TWEAK", Label.AttractTweaker },
        { "LABEL_HEROCAPE", Label.HeroCape },
        { "LABEL_MEMORISER", Label.Memoriser },
        { "LABEL_WALLJUMP", Label.WallJump },
        { "LABEL_INTERACTIVE_STREAM", Label.InteractiveStream },
        { "LABEL_QUESTS", Label.Quests },
        { "LABEL_SACKPOCKET", Label.Sackpocket },
        { "LABEL_SPRINGINATOR", Label.Springinator },
        { "LABEL_HOVERBOARD_NAME", Label.Hoverboard },

        { "LABEL_FLOATY_FLUID_NAME", Label.FloatyFluid },
        { "LABEL_ODDSOCK", Label.Oddsock },
        { "LABEL_TOGGLE", Label.Toggle },
        { "LABEL_SWOOP", Label.Swoop },
        { "LABEL_SACKBOY", Label.Sackboy },
        { "LABEL_CREATED_CHARACTERS", Label.CreatedCharacters },
        { "LABEL_Flick", Label.Flick },
        { "LABEL_MultiLevel", Label.MultiLevel },
        { "LABEL_Portrait", Label.Portrait },
        { "LABEL_RearTouch", Label.RearTouch },

        { "LABEL_SharedScreen", Label.SharedScreen },
        { "LABEL_Swipe", Label.Swipe },
        { "LABEL_Tap", Label.Tap },
        { "LABEL_Tilt", Label.Tilt },
        { "LABEL_Touch", Label.Touch },
    };

    public static string? ToLbpString(this Label label) => StringMap.GetValueOrDefault(label);

    public static Label? FromLbpString(string str) => LabelsMap.GetValueOrDefault(str);

    public static List<string> ToLbpList(this List<Label> labels)
        => labels.Select(l => l.ToLbpString()!).ToList();

    public static List<Label> FromLbpList(List<string> labels)
    {
        List<Label> finalLabels = [];

        foreach (string label in labels)
        {
            Label? deserialized = FromLbpString(label);
            if (deserialized != null) finalLabels.Add(deserialized.Value);
        }

        return finalLabels;
    }
}