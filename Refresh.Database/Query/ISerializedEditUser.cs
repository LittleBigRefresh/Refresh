using Refresh.Database.Models;

namespace Refresh.Database.Query;

public interface ISerializedEditUser
{
    string? Description { get; set; }
    GameLocation? UserLocation { get; set; }
    string? PlanetsHash { get; set; }
    string? IconHash { get; set; }
    string? YayFaceHash { get; set; }
    string? BooFaceHash { get; set; }
    string? MehFaceHash { get; set; }
}