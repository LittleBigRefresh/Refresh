namespace Refresh.Database;

public interface ISequentialId
{
    [NotMapped]
    int SequentialId { get; set; }
}