using Refresh.Core.Types.Data;
using Refresh.Database;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Refresh.Interfaces.APIv3.Extensions;

public static class DatabaseListExtensions
{
    public static DatabaseList<TNewObject> FromOldList<TNewObject, TOldObject>(DatabaseList<TOldObject> oldList, DataContext dataContext)
        where TNewObject : class, IDataConvertableFrom<TNewObject, TOldObject>
        where TOldObject : class
    {
        DatabaseList<TNewObject> newList = new(oldList.TotalItems, oldList.NextPageIndex, TNewObject.FromOldList(oldList.Items.ToArray(), dataContext));
        return newList;
    }
}