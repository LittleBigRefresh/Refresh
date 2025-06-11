using System.Diagnostics;
using System.Reflection;

namespace Refresh.Database.Extensions;


public static class RealmObjectExtensions
{
    public static IRealmObject Clone(this IRealmObject source, bool deep = true)
    {
        Type type = source.GetType();
        
        IRealmObject? clone = (IRealmObject?)Activator.CreateInstance(type);
        Debug.Assert(clone != null);

        foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if(prop.IsDefined(typeof(IgnoredAttribute))) continue;
            
            if(!prop.CanWrite || !prop.CanRead) continue;

            object? value = prop.GetValue(source);

            if (value is IRealmObject obj && deep)
                prop.SetValue(clone, obj.Clone());
            else
                prop.SetValue(clone, value);
        }

        #if !POSTGRES
        Debug.Assert(!clone.IsManaged);
        #endif
        return clone;
    }
}