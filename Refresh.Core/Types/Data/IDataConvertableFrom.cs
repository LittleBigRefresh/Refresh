namespace Refresh.Core.Types.Data;

public interface IDataConvertableFrom<out TNew, in TOld> where TNew : IDataConvertableFrom<TNew, TOld>
{
    public static abstract TNew? FromOld(TOld? old, DataContext dataContext);

    public static abstract IEnumerable<TNew> FromOldList(IEnumerable<TOld> oldList, DataContext dataContext);
}