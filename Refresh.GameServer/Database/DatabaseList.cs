namespace Refresh.GameServer.Database;

public class DatabaseList<TObject> where TObject : class
{
    // ReSharper disable once ParameterTypeCanBeEnumerable.Local
    public DatabaseList(IQueryable<TObject> items, int skip, int count)
    {
        this.Items = items.AsEnumerable().Skip(skip).Take(count);
        this.TotalItems = items.Count();
        this.NextPageIndex = skip + count + 1;

        if (this.NextPageIndex > this.TotalItems)
            this.NextPageIndex = 0;
    }
    
    public DatabaseList(IEnumerable<TObject> items)
    {
        IEnumerable<TObject> itemsList = items.ToList();
        
        this.Items = itemsList;
        this.TotalItems = itemsList.Count();
        this.NextPageIndex = -1;
    }

    public static DatabaseList<TObject> Empty() => new(Array.Empty<TObject>());

    public IEnumerable<TObject> Items { get; private init; }
    public int TotalItems { get; }
    public int NextPageIndex { get; }
}