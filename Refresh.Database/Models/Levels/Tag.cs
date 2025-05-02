using System.Text;

namespace Refresh.GameServer.Types.Levels;

public enum Tag : byte
{
    Boss = 0,
    Varied = 1,
    Repetitive = 2,
    MultiPath = 3,
    SinglePath = 4,
    Frustrating = 5,
    Relaxing = 6,
    Coop = 7,
    Competitive = 8,
    Fun = 9,
    Funny = 10,
    Complex = 11,
    Simple = 12,
    Long = 13,
    Short = 14,
    Quick = 15,
    Slow = 16,
    Tricky = 17,
    Horizontal = 18,
    Vertical = 19,
    Musical = 20,
    Moody = 21,
    Timing = 22,
    Perilous = 23,
    NerveWracking = 24,
    Cute = 25,
    Mad = 26,
    Hectic = 27,
    Creepy = 28,
    Daft = 29,
    Hilarious = 30,
    Puzzler = 31,
    Platformer = 32,
    Speedy = 33,
    Fast = 34,
    PointsFest = 35,
    Artistic = 36,
    Funky = 37,
    Empty = 38,
    Mechanical = 39,
    Race = 40,
    Fiery = 41,
    Spikes = 42,
    Vehicles = 43,
    Ramps = 44,
    Machines = 45,
    Toys = 46,
    Stickers = 47,
    Gas = 48,
    Secrets = 49,
    Collectables = 50,
    Braaains = 51,
    Hoists = 52,
    Bubbly = 53,
    Swingy = 54,
    Balancing = 55,
    Floaty = 56,
    Springy = 57,
    Machinery = 58,
    Annoying = 59,
    Satisfying = 60,
    Brilliant = 61,
    Great = 62,
    Good = 63,
    Rubbish = 64,
    Pretty = 65,
    Ugly = 66,
    Difficult = 67,
    Easy = 68,
    Weird = 69,
    Boring = 70,
    Splendid = 71,
    Lousy = 72,
    Ingenious = 73,
    Beautiful = 74,
    Electric = 75,
}

public static class TagExtensions
{
    static TagExtensions()
    {
        StringBuilder allTags = new();
        
        // Create the conversion which goes the other way
        foreach ((string? key, Tag value) in TagsMap)
        {
            StringMap[value] = key;

            allTags.Append(key);
            allTags.Append(',');
        }

        if (allTags.Length > 1)
            allTags.Remove(allTags.Length - 1, 1);

        AllTags = allTags.ToString();
    }

    public static string AllTags { get; private set; }
    
    // C# doesnt seem to have a better construct for this...
    private static readonly Dictionary<Tag, string> StringMap = new();
    private static readonly Dictionary<string, Tag> TagsMap = new()
    {
        { "TAG_Boss", Tag.Boss },
        { "TAG_Varied", Tag.Varied },
        { "TAG_Repetitive", Tag.Repetitive },
        { "TAG_Multi-Path", Tag.MultiPath },
        { "TAG_Single-Path", Tag.SinglePath },
        { "TAG_Frustrating", Tag.Frustrating },
        { "TAG_Relaxing", Tag.Relaxing },
        { "TAG_Co-op", Tag.Coop },
        { "TAG_Competitive", Tag.Competitive },
        { "TAG_Fun", Tag.Fun },
        { "TAG_Funny", Tag.Funny },
        { "TAG_Complex", Tag.Complex },
        { "TAG_Simple", Tag.Simple },
        { "TAG_Long", Tag.Long },
        { "TAG_Short", Tag.Short },
        { "TAG_Quick", Tag.Quick },
        { "TAG_Slow", Tag.Slow },
        { "TAG_Tricky", Tag.Tricky },
        { "TAG_Horizontal", Tag.Horizontal },
        { "TAG_Vertical", Tag.Vertical },
        { "TAG_Musical", Tag.Musical },
        { "TAG_Moody", Tag.Moody },
        { "TAG_Timing", Tag.Timing },
        { "TAG_Perilous", Tag.Perilous },
        { "TAG_Nerve-wracking", Tag.NerveWracking },
        { "TAG_Cute", Tag.Cute },
        { "TAG_Mad", Tag.Mad },
        { "TAG_Hectic", Tag.Hectic },
        { "TAG_Creepy", Tag.Creepy },
        { "TAG_Daft", Tag.Daft },
        { "TAG_Hilarious", Tag.Hilarious },
        { "TAG_Puzzler", Tag.Puzzler },
        { "TAG_Platformer", Tag.Platformer },
        { "TAG_Speedy", Tag.Speedy },
        { "TAG_Fast", Tag.Fast },
        { "TAG_Points-Fest", Tag.PointsFest },
        { "TAG_Artistic", Tag.Artistic },
        { "TAG_Funky", Tag.Funky },
        { "TAG_Empty", Tag.Empty },
        { "TAG_Mechanical", Tag.Mechanical },
        { "TAG_Race", Tag.Race },
        { "TAG_Fiery", Tag.Fiery },
        { "TAG_Spikes", Tag.Spikes },
        { "TAG_Vehicles", Tag.Vehicles },
        { "TAG_Ramps", Tag.Ramps },
        { "TAG_Machines", Tag.Machines },
        { "TAG_Toys", Tag.Toys },
        { "TAG_Stickers", Tag.Stickers },
        { "TAG_Gas", Tag.Gas },
        { "TAG_Secrets", Tag.Secrets },
        { "TAG_Collectables", Tag.Collectables },
        { "TAG_Braaains", Tag.Braaains },
        { "TAG_Hoists", Tag.Hoists },
        { "TAG_Bubbly", Tag.Bubbly },
        { "TAG_Swingy", Tag.Swingy },
        { "TAG_Balancing", Tag.Balancing },
        { "TAG_Floaty", Tag.Floaty },
        { "TAG_Springy", Tag.Springy },
        { "TAG_Machinery", Tag.Machinery },
        { "TAG_Annoying", Tag.Annoying },
        { "TAG_Satisfying", Tag.Satisfying },
        { "TAG_Brilliant", Tag.Brilliant },
        { "TAG_Great", Tag.Great },
        { "TAG_Good", Tag.Good },
        { "TAG_Rubbish", Tag.Rubbish },
        { "TAG_Pretty", Tag.Pretty },
        { "TAG_Ugly", Tag.Ugly },
        { "TAG_Difficult", Tag.Difficult },
        { "TAG_Easy", Tag.Easy },
        { "TAG_Weird", Tag.Weird },
        { "TAG_Boring", Tag.Boring },
        { "TAG_Splendid", Tag.Splendid },
        { "TAG_Lousy", Tag.Lousy },
        { "TAG_Ingenious", Tag.Ingenious },
        { "TAG_Beautiful", Tag.Beautiful },
        { "TAG_Electric", Tag.Electric },
    };

    public static string? ToLbpString(this Tag tag) => StringMap.GetValueOrDefault(tag);

    public static Tag? FromLbpString(string str) => TagsMap.GetValueOrDefault(str);
}