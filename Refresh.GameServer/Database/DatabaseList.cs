namespace Refresh.GameServer.Database;

public class DatabaseList<TObject> where TObject : class
{
    public DatabaseList(IQueryable<TObject> items)
    {
        this.Items = items.AsEnumerable();
        this.TotalItems = items.Count();
    }
    
    // ReSharper disable once ParameterTypeCanBeEnumerable.Local
    public DatabaseList(IQueryable<TObject> items, int skip, int count)
    {
        this.Items = items.AsEnumerable().Skip(skip).Take(count);
        this.TotalItems = skip + count;
    }
    
    public DatabaseList(IEnumerable<TObject> items)
    {
        IEnumerable<TObject> itemsList = items.ToList();
        
        this.Items = itemsList;
        this.TotalItems = itemsList.Count();
    }

    public static DatabaseList<TObject> Empty() => new(Array.Empty<TObject>());

    public IEnumerable<TObject> Items { get; private init; }
    public int TotalItems { get; private init; }
}