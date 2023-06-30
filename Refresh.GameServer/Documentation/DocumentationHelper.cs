using System.Reflection;
using System.Runtime.CompilerServices;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Extensions;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints;

namespace Refresh.GameServer.Documentation;

public static class DocumentationHelper
{
    private static readonly List<DocumentationRoute> _docs = new();
    public static IEnumerable<DocumentationRoute> Documentation => _docs.AsReadOnly(); 
    
    static DocumentationHelper()
    {
        List<MethodInfo> methods = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(EndpointGroup)))
            .SelectMany(t => t.GetMethods())
            .Where(m => m.HasCustomAttribute<ApiV3EndpointAttribute>())
            .ToList();

        foreach (MethodInfo method in methods)
        {
            ApiV3EndpointAttribute endpoint = method.GetCustomAttribute<ApiV3EndpointAttribute>()!;
            string summary = method.GetCustomAttribute<DocSummaryAttribute>()?.Summary ?? "No summary provided.";
            
            DocumentationRoute route = new(endpoint.RouteWithParameters, summary);

            foreach (DocErrorAttribute error in method.GetCustomAttributes<DocErrorAttribute>())
            {
                DocumentationError docError = new(error.ErrorType.Name, error.When);
                route.PotentialErrors.Add(docError);
            }
            
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                DocSummaryAttribute? paramSummary = parameter.GetCustomAttribute<DocSummaryAttribute>();
                if(paramSummary == null) continue;

                DocumentationParameter docParameter = new(parameter.Name!, ParameterType.Route, paramSummary.Summary);
                route.Parameters.Add(docParameter);
            }

            AuthenticationAttribute? authentication = method.GetCustomAttribute<AuthenticationAttribute>();
            route.AuthenticationRequired = authentication == null || authentication.Required;

            if (method.HasCustomAttribute<DocUsesPageDataAttribute>())
            {
                route.Parameters.Add(new DocumentationParameter("skip", ParameterType.Query, "The index of the list to skip to"));
                route.Parameters.Add(new DocumentationParameter("count", ParameterType.Query, "The amount of items to take from the list"));
            }
            
            _docs.Add(route);
        }
    }
    
    public static void WriteDocumentationAsJson(string path = ".")
    {
        string json = JsonConvert.SerializeObject(_docs, Formatting.Indented);
        File.WriteAllText(Path.Combine(path, "apiDocumentation.json"), json);
    }
}