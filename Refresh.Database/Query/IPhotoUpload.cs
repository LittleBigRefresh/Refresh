namespace Refresh.Database.Query;

public interface IPhotoUpload
{
    int? LevelId { get; }
    string? LevelType { get; }
    string? LevelTitle { get; }
    string SmallHash { get; }
    string MediumHash { get;}
    string LargeHash { get; }
    string PlanHash { get; }
    long Timestamp { get; }
}