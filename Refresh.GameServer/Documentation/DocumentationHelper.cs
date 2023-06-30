using System.Reflection;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Extensions;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints;

namespace Refresh.GameServer.Documentation;

public static class DocumentationHelper
{
    private static readonly List<DocumentationRoute> _docs = new();
    
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
            
            DocumentationRoute route = new(endpoint.FullRoute, summary);

            foreach (DocErrorAttribute error in method.GetCustomAttributes<DocErrorAttribute>())
            {
                DocumentationError docError = new(error.ErrorType.Name, error.When);
                route.PotentialErrors.Add(docError);
            }
            
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                DocParamAttribute? parameterAttribute = parameter.GetCustomAttribute<DocParamAttribute>();
                if(parameterAttribute == null) continue;

                DocumentationParameter docParameter = new(parameter.Name!, parameterAttribute.Summary);
                route.Parameters.Add(docParameter);
            }
            
            _docs.Add(route);
        }
    }
    
    public static void WriteDocumentationAsJson(string path = ".")
    {
        string json = JsonConvert.SerializeObject(_docs, Formatting.Indented);
        File.WriteAllText(Path.Combine(path, "docs.json"), json);
    }
}