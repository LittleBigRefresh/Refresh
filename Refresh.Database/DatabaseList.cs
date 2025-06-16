namespace Refresh.Database;

public class DatabaseList<TObject> where TObject : class
{
    // ReSharper disable once ParameterTypeCanBeEnumerable.Local
    public DatabaseList(IQueryable<TObject> items, int skip, int count)
    {
        this.Items = items.AsEnumerableIfRealm().Skip(skip).Take(count);
        this.TotalItems = items.Count();
        this.NextPageIndex = skip + count + 1;

        if (this.NextPageIndex > this.TotalItems)
            this.NextPageIndex = 0;
    }
    
    public DatabaseList(IEnumerable<TObject> items, int skip, int count)
    {
        this.Items = items.AsEnumerableIfRealm().Skip(skip).Take(count);
        this.TotalItems = items.Count();
        this.NextPageIndex = skip + count + 1;

        if (this.NextPageIndex > this.TotalItems)
            this.NextPageIndex = 0;
    }
    
    public DatabaseList(IEnumerable<TObject> items)
    {
        this.Items = items;
        this.TotalItems = items.Count();
        this.NextPageIndex = -1;
    }

    [Obsolete("For conversion use only.")]
    public DatabaseList(int oldTotalItems, int oldNextPageIndex, IEnumerable<TObject> items)
    {
        this.Items = items.ToList();
        this.TotalItems = oldTotalItems;
        this.NextPageIndex = oldNextPageIndex;
    }

    public static DatabaseList<TObject> Empty() => new([]);

    public IEnumerable<TObject> Items { get; private init; }
    public int TotalItems { get; }
    public int NextPageIndex { get; }
}