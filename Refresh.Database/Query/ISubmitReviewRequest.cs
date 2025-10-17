using Refresh.Database.Models.Levels;

namespace Refresh.Database.Query;

public interface ISubmitReviewRequest
{
    public List<Label>? Labels { get; set; }
    public string? Content { get; set; }
}