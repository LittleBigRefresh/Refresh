using System.Reflection;
using AttribDoc;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Extensions;
using Refresh.GameServer.Endpoints;

namespace Refresh.GameServer.Documentation;

public class RefreshDocumentationGenerator : DocumentationGenerator
{
    protected override IEnumerable<MethodInfo> FindMethodsToDocument(Assembly assembly) 
        => assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(EndpointGroup)))
            .SelectMany(t => t.GetMethods())
            .Where(m => m.HasCustomAttribute<ApiV3EndpointAttribute>())
            .ToList();

    protected override void DocumentRouteHook(MethodInfo method, Route route)
    {
        ApiV3EndpointAttribute endpoint = method.GetCustomAttribute<ApiV3EndpointAttribute>()!;
        route.Summary = "No summary provided.";

        route.Method = endpoint.Method.ToString().ToUpper();
        route.RouteUri = endpoint.RouteWithParameters;
        
        AuthenticationAttribute? authentication = method.GetCustomAttribute<AuthenticationAttribute>();
        route.AuthenticationRequired = authentication == null || authentication.Required;

        if (route.AuthenticationRequired)
        {
            MinimumRoleAttribute? roleAttribute = method.GetCustomAttribute<MinimumRoleAttribute>();
            if (roleAttribute != null)
                route.ExtraProperties["minimumRole"] = roleAttribute.MinimumRole;

            RequireEmailVerifiedAttribute? emailAttribute = method.GetCustomAttribute<RequireEmailVerifiedAttribute>();
            if(emailAttribute != null)
                route.ExtraProperties["emailRequired"] = true;
        }
    }
}