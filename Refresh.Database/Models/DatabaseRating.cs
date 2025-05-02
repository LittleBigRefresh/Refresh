namespace Refresh.Database.Models;

/// <summary>
/// This object is used to model anything in the game that requires a positive and negative rating. Some uses
/// could be in Levels or Reviews where both have a Yay and Boo button ingame, for instance.
/// </summary>
public class DatabaseRating
{
    public int PositiveRating { get; set; } = 0;
    public int NegativeRating { get; set; } = 0;
}