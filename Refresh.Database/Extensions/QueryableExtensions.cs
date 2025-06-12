namespace Refresh.Database.Extensions;

public static class QueryableExtensions
{
#if !POSTGRES
    public static IQueryable<T> Include<T>(this IQueryable<T> queryable, Func<T, object?> func)
    {
        _ = func;
        return queryable;
    }
#endif
}