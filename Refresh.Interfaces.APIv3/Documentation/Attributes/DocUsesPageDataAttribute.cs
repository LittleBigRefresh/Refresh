using System.Reflection;
using AttribDoc;
using AttribDoc.Attributes;

namespace Refresh.Interfaces.APIv3.Documentation.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class DocUsesPageDataAttribute : DocAttribute
{
    public override void AddDataToRouteDocumentation(MethodInfo method, Route route)
    {
        route.Parameters.Add(new Parameter("skip", ParameterType.Query, "The amount of items to skip over - where you are in the list"));
        route.Parameters.Add(new Parameter("count", ParameterType.Query, "The amount of items to take from the list"));
    }
}