namespace Refresh.Database.Extensions;

public static class QueryableExtensions
{
#if !POSTGRES
    public static IQueryable<T> Include<T>(this IQueryable<T> queryable, Func<T, object?> func)
    {
        _ = func;
        return queryable;
    }

    public static IEnumerable<T> AsEnumerableIfRealm<T>(this IQueryable<T> queryable)
    {
        return queryable.AsEnumerable();
    }
    
    public static IEnumerable<T> AsEnumerableIfRealm<T>(this IEnumerable<T> queryable)
    {
        return queryable.AsEnumerable();
    }
#else
    public static IQueryable<T> AsEnumerableIfRealm<T>(this IQueryable<T> queryable)
    {
        return queryable;
    }
    
    public static IEnumerable<T> AsEnumerableIfRealm<T>(this IEnumerable<T> queryable)
    {
        return queryable;
    }
#endif
}