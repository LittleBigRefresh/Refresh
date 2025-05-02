namespace Refresh.Database.Query;

public interface IApiEditLevelRequest
{
    string? Title { get; set; }
    string? Description { get; set; }
    string? IconHash { get; set; }
}