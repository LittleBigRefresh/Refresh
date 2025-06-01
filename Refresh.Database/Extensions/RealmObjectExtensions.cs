#if !POSTGRES
using System.Diagnostics;
using System.Reflection;

namespace Refresh.Database.Extensions;


public static class RealmObjectExtensions
{
    public static IRealmObjectBase Clone(this IRealmObjectBase source, bool deep = true)
    {
        Type type = source.GetType();
        
        IRealmObjectBase? clone = (IRealmObjectBase?)Activator.CreateInstance(type);
        Debug.Assert(clone != null);

        foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if(prop.IsDefined(typeof(IgnoredAttribute))) continue;
            if(prop.IsDefined(typeof(BacklinkAttribute))) continue;
            
            if(!prop.CanWrite || !prop.CanRead) continue;

            object? value = prop.GetValue(source);

            if (value is IRealmObjectBase obj && deep)
                prop.SetValue(clone, obj.Clone());
            else
                prop.SetValue(clone, value);
        }

        Debug.Assert(!clone.IsManaged);
        return clone;
    }
}

#endif