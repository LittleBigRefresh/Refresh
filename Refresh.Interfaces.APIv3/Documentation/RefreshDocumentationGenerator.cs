using System.Reflection;
using AttribDoc;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Extensions;
using Refresh.Core.Authentication.Permission;

namespace Refresh.Interfaces.APIv3.Documentation;

public class RefreshDocumentationGenerator : DocumentationGenerator
{
    protected override IEnumerable<MethodInfo> FindMethodsToDocument(Assembly assembly) 
        => assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(EndpointGroup)))
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes<ApiV3EndpointAttribute>().Any())
            .ToList();

    protected override void DocumentRouteHook(MethodInfo method, Route route)
    {
        IEnumerable<ApiV3EndpointAttribute> endpoints = method.GetCustomAttributes<ApiV3EndpointAttribute>();
        route.Summary = "No summary provided.";

        // TODO modify AttribDoc to actually show all routes associated with this method
        // TODO also allow AttribDoc to group endpoints together by class, HTTP method, auth requirement etc.
        // so they're not all over the place on the website.
        route.Method = endpoints.First().Method.ToString().ToUpper();
        route.RouteUri = endpoints.First().RouteWithParameters;
        
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