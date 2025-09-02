using System.Text;

namespace Refresh.Database.Models.Levels;

public enum Label : byte
{
    // Category 1 - Experience
    Short,
    Long,
    Challenging,
    Easy,
    Scary,
    Funny,
    Artistic,
    Musical,
    Intricate,
    Singleplayer,
    Multiplayer,
    RPG,
    // Category 2 - Type
    Cinematic,
    Versus,
    Fighter,
    Gallery,
    Puzzler,
    Platformer,
    Racer,
    Shooter,
    Sports,
    Story,
    Strategy,
    SurvivalChallenge,
    Tutorial,
    Retro,
    TopDown,
    Cooperative,
    FirstPerson,
    ThirdPerson,
    SciFi,
    Social,
    ArcadeGame,
    BoardGame,
    CardGame,
    MiniGame,
    PartyGame,
    Defence,
    Driving,
    Hangout,
    HideAndSeek,
    PropHunt,
    MusicGallery,
    CostumeGallery,
    StickerGallery,
    Movie,
    Pinball,
    Technology,
    Homage,
    EightBit,
    SixteenBit,
    Seasonal,
    TimeTrial,
    // Category 3 - Content
    Collectables, 
    Controlinator,
    Explosives,
    Glitch,
    GrapplingHook,
    BouncePads,
    Creatinator,
    LowGravity,
    Paintinator,
    Grabinators,
    Sackbots,
    Vehicles,
    Water,
    BrainCrane,
    Movinator,
    Paint,
    AttractGel,
    AttractTweaker,
    HeroCape,
    Memoriser,
    WallJump,
    InteractiveStream,
    Quests,
    Sackpocket,
    Springinator,
    Hoverboard,
    FloatyFluid,
    // Category 4 - Characters
    Oddsock,
    Toggle,
    Swoop,
    Sackboy,
    CreatedCharacters,
    // LBP Vita exclusive
    Precision,
    Flick,
    RearTouch,
    SharedScreen,
    Swipe,
    Tap,
    Tilt,
    Touch,
    Portrait,
    MultiLevel,
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
    
    private static readonly Dictionary<Label, string> StringMap = [];
    private static readonly Dictionary<string, Label> LabelsMap = new()
    {
        { "LABEL_Quick", Label.Short },
        { "LABEL_Long", Label.Long },
        { "LABEL_Challenging", Label.Challenging },
        { "LABEL_Easy", Label.Easy },
        { "LABEL_Scary", Label.Scary },
        { "LABEL_Funny", Label.Funny },
        { "LABEL_Artistic", Label.Artistic },
        { "LABEL_Musical", Label.Musical },
        
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

        // LBP2/Vita exclusive
        { "LABEL_Intricate", Label.Intricate },

        // Not selectable, but still in LBP2
        { "LABEL_SinglePlayer", Label.Singleplayer },
        { "LABEL_Multiplayer", Label.Multiplayer },

        // LBP3 exclusive
        { "LABEL_SINGLE_PLAYER", Label.Singleplayer },
        { "LABEL_RPG", Label.RPG },
        { "LABEL_TOP_DOWN", Label.TopDown },
        { "LABEL_CO_OP", Label.Cooperative },
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

        // LBP Vita exclusive
        { "LABEL_Co-op", Label.Cooperative },
        { "LABEL_Precision", Label.Precision },
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

    public static string ToLbpCommaList(this IEnumerable<Label> labels)
        => string.Join(',', labels.Select(l => l.ToLbpString()!));

    public static IEnumerable<Label> FromLbpCommaList(string labels)
    {
        string[] labelStrings = labels.Split(',');
        IEnumerable<Label> finalLabels = [];

        foreach (string label in labelStrings)
        {
            Label? deserialized = FromLbpString(label);
            if (deserialized != null) finalLabels = finalLabels.Append(deserialized.Value);
        }

        return finalLabels;
    }
}