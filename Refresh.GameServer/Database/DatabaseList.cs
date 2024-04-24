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
    
    public DatabaseList(IEnumerable<TObject> items, int skip, int count)
    {
        List<TObject> itemsList = items.ToList();
        
        this.Items = itemsList.Skip(skip).Take(count);
        this.TotalItems = itemsList.Count;
        this.NextPageIndex = skip + count + 1;

        if (this.NextPageIndex > this.TotalItems)
            this.NextPageIndex = 0;
    }
    
    public DatabaseList(IEnumerable<TObject> items)
    {
        List<TObject> itemsList = items.ToList();
        
        this.Items = itemsList;
        this.TotalItems = itemsList.Count;
        this.NextPageIndex = -1;
    }

    private DatabaseList(int oldTotalItems, int oldNextPageIndex, IEnumerable<TObject> items)
    {
        this.Items = items.ToList();
        this.TotalItems = oldTotalItems;
        this.NextPageIndex = oldNextPageIndex;
    }

    // public static DatabaseList<TObject> FromOldList<TObjectOld>(DatabaseList<TObjectOld> oldList, Func<TObjectOld, TObject> selector) where TObjectOld : class
    // {
    //     List<TObjectOld> oldItems = oldList.Items.ToList();
    //     
    //     List<TObject> items = new(oldItems.Count);
    //     items.AddRange(oldItems.Select(selector.Invoke));
    //
    //     DatabaseList<TObject> newList = new(oldList.TotalItems, oldList.NextPageIndex, items);
    //     return newList;
    // }
    
    public static DatabaseList<TNewObject> FromOldList<TNewObject, TOldObject>(DatabaseList<TOldObject> oldList, Func<TOldObject?, TNewObject?> mapFunc)
        where TNewObject : class
        where TOldObject : class
    {
        DatabaseList<TNewObject> newList = new(oldList.TotalItems, oldList.NextPageIndex, oldList.Items.Select(mapFunc)!);
        return newList;
    }

    public static DatabaseList<TObject> Empty() => new(Array.Empty<TObject>());

    public IEnumerable<TObject> Items { get; private init; }
    public int TotalItems { get; }
    public int NextPageIndex { get; }
}