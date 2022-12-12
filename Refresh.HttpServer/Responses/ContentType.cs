using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Refresh.HttpServer.Responses;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum ContentType
{
    [ContentTypeName("text/html")] Html,
    [ContentTypeName("text/plain")] Plaintext,
    [ContentTypeName("text/xml")] Xml,
    [ContentTypeName("text/json")] Json,
}

[AttributeUsage(AttributeTargets.Field)]
internal class ContentTypeNameAttribute : Attribute
{
    internal string Name { get; }

    internal ContentTypeNameAttribute(string name)
    {
        this.Name = name;
    }
}

internal static class ContentTypeExtensions
{
    internal static string GetName(this ContentType contentType)
    {
        Type type = typeof(ContentType);
        
        MemberInfo? memberInfo = type.GetMember(contentType.ToString()).FirstOrDefault();
        Debug.Assert(memberInfo != null);
        
        ContentTypeNameAttribute? attribute = memberInfo.GetCustomAttribute<ContentTypeNameAttribute>();
        Debug.Assert(attribute != null);

        return attribute.Name;
    }
}