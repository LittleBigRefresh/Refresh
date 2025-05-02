namespace Refresh.Database.Models.Comments;

public enum RatingType : sbyte
{
    Yay = 1,
    Neutral = 0,
    Boo = -1,
}

public static class RatingTypeExtensions
{
    public static int ToDPad(this RatingType type)
    {
        return (int)type;
    }

    public static int ToLBP1(this RatingType type)
    {
        return type switch {
            RatingType.Yay => 5,
            RatingType.Neutral => 3,
            RatingType.Boo => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}