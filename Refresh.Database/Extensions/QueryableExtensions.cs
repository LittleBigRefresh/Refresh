namespace Refresh.Database.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> AsEnumerableIfRealm<T>(this IQueryable<T> queryable)
    {
        return queryable;
    }
    
    public static IEnumerable<T> AsEnumerableIfRealm<T>(this IEnumerable<T> queryable)
    {
        return queryable;
    }

    public static IEnumerable<T> ToArrayIfPostgres<T>(this IQueryable<T> queryable)
    {
        return queryable.ToArray();
    }
    
    public static IEnumerable<T> ToArrayIfPostgres<T>(this IEnumerable<T> queryable)
    {
        return queryable.ToArray();
    }
}