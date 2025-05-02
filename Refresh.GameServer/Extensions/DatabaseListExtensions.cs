using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.Data;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Refresh.GameServer.Extensions;

public static class DatabaseListExtensions
{
    public static DatabaseList<TNewObject> FromOldList<TNewObject, TOldObject>(DatabaseList<TOldObject> oldList, DataContext dataContext)
        where TNewObject : class, IDataConvertableFrom<TNewObject, TOldObject>
        where TOldObject : class
    {
        DatabaseList<TNewObject> newList = new(oldList.TotalItems, oldList.NextPageIndex, TNewObject.FromOldList(oldList.Items, dataContext));
        return newList;
    }
}