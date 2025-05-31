namespace Refresh.Database.Models.Activity;

#nullable disable

public abstract class DatabaseActivityGroup
{
    public abstract string GroupType { get; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.MinValue;
    public List<Event> Events { get; set; } = [];
    public List<DatabaseActivityGroup> Children { get; set; } = [];
    
    #nullable enable

    internal List<DatabaseActivityUserGroup>? UserChildren = [];

    internal void Cleanup()
    {
        foreach (DatabaseActivityGroup group in this.Children)
            group.Cleanup();

        this.UserChildren = null;
    }
}