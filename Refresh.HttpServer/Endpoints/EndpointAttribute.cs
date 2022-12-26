using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using JetBrains.Annotations;
using Refresh.HttpServer.Responses;

namespace Refresh.HttpServer.Endpoints;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class EndpointAttribute : Attribute
{
    public readonly string FullRoute;
    private readonly Dictionary<int, string> _parameterIndexes = new();

    public readonly Method Method;
    public readonly ContentType ContentType;

    public EndpointAttribute(string route, Method method = Method.Get, ContentType contentType = ContentType.Plaintext)
    {
        this.Method = method;
        this.ContentType = contentType;

        // Skip scanning for parameters if the route obviously doesn't contain one
        // Maybe can optimize further in the future with source generators?
        // Only runs once per endpoint either way, so whatever
        if (route.IndexOf('{') == -1)
        {
            this.FullRoute = route;
            return;
        }
        
        // Scan for route parameters

        string fullRoute = string.Empty;
        
        string[] routeSplit = route.Split('/');
        for (int i = 0; i < routeSplit.Length; i++)
        {
            string s = routeSplit[i];
            if (i != 0) fullRoute += '/';
            
            if (s.StartsWith('{') && s.EndsWith('}'))
            {
                this._parameterIndexes.Add(i, s.Substring(1, s.Length - 2));
                fullRoute += '_';
            }
            else
            {
                fullRoute += s;
            }
        }

        this.FullRoute = fullRoute;
    }

    public EndpointAttribute(string route, ContentType contentType, Method method = Method.Get)
        : this(route, method, contentType)
    {
        
    }

    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public bool UriMatchesRoute(Uri? uri, Method method, out Dictionary<string, string> parameters)
    {
        parameters = new Dictionary<string, string>();
        if (uri == null) return false;
        
        string path = uri.AbsolutePath;

        if (method != this.Method) return false;
        if (this.FullRoute == path) return true;
        if (this._parameterIndexes.Count != 0)
        {
            string fullRoute = string.Empty;
            string[] routeSplit = path.Split('/');
            for (int i = 0; i < routeSplit.Length; i++)
            {
                string s = routeSplit[i];
                if (i != 0) fullRoute += '/';
                
                if (!this._parameterIndexes.TryGetValue(i, out string? paramName))
                {
                    fullRoute += s;
                }
                else
                {
                    Debug.Assert(paramName != null);
                    parameters.Add(paramName, HttpUtility.UrlDecode(s));

                    fullRoute += "_";
                }
            }

            return fullRoute == this.FullRoute;
        }
        
        return false;
    }
}