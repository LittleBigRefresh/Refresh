using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.Data;

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
    
    public static DatabaseList<TNewObject> FromOldList<TNewObject, TOldObject>(DatabaseList<TOldObject> oldList, DataContext dataContext)
        where TNewObject : class, IDataConvertableFrom<TNewObject, TOldObject>
        where TOldObject : class
    {
        DatabaseList<TNewObject> newList = new(oldList.TotalItems, oldList.NextPageIndex, TNewObject.FromOldList(oldList.Items, dataContext));
        return newList;
    }

    public static DatabaseList<TObject> Empty() => new(Array.Empty<TObject>());

    public IEnumerable<TObject> Items { get; private init; }
    public int TotalItems { get; }
    public int NextPageIndex { get; }
}