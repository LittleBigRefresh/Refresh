using System.Text;
using System.Web;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Bunkum.Listener.Request;
using Refresh.HttpsProxy.Config;

namespace Refresh.HttpsProxy.Middlewares;

public class ProxyMiddleware(ProxyConfig config) : IMiddleware
{
    private readonly ThreadLocal<HttpClient> _httpClients = new(() => new HttpClient());
    
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        HttpClient client = this._httpClients.Value!;

        UriBuilder uri = new(config.TargetServerUrl)
        {
            Path = context.Uri.AbsolutePath,
        };

        StringBuilder queryString = new();

        // We apparently need to prepend this ourselves
        if (context.Query.Count > 0) 
            queryString.Append('?');

        // Fill in all the query strings
        for (int i = 0; i < context.Query.Count; i++)
        {
            string key = context.Query.GetKey(i)!;
            string name = HttpUtility.HtmlEncode(key);
            string value = HttpUtility.HtmlEncode(context.Query[key]!);

            queryString.AppendFormat("{0}={1}", name, value);

            if (i < context.Query.Count - 1)
                queryString.Append('&');
        }
        
        uri.Query = queryString.ToString();

        HttpRequestMessage requestMessage = new()
        {
            RequestUri = uri.Uri,
            Content = new StreamContent(context.InputStream), 
            Method = new HttpMethod(context.Method.Value), 
            Version = new Version(1, 1), 
            VersionPolicy = HttpVersionPolicy.RequestVersionExact,
        };

        // Move all the request headers over
        for (int i = 0; i < context.RequestHeaders.Count; i++)
        {
            string key = context.RequestHeaders.GetKey(i)!;
            string[] value = context.RequestHeaders.GetValues(i)!;

            // Fixup the host value to the correct one, so that the request actually goes through
            if (key.Equals("Host", StringComparison.InvariantCultureIgnoreCase))
                value = [uri.Host];
            
            requestMessage.Headers.TryAddWithoutValidation(key, value);
        }

        requestMessage.Headers.Add("Refresh-Ps3-Digest-Index", config.Ps3DigestIndex.ToString());
        requestMessage.Headers.Add("Refresh-Ps4-Digest-Index", config.Ps4DigestIndex.ToString());

        // Send our HTTP request
        HttpResponseMessage response = client.Send(requestMessage);

        //Set the response information
        context.ResponseCode = response.StatusCode;
        
        context.ResponseHeaders.Clear();
        foreach (KeyValuePair<string, IEnumerable<string>> responseHeader in response.Headers)
        {
            context.ResponseHeaders[responseHeader.Key] = string.Join(',', responseHeader.Value);
        }
        context.ResponseStream.SetLength(0);
        response.Content.ReadAsStream().CopyTo(context.ResponseStream);
    }
}