using System.Security.Cryptography;
using System.Text;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Bunkum.Listener.Request;
using Refresh.Common.Extensions;
using Refresh.HttpsProxy.Config;

namespace Refresh.HttpsProxy.Middlewares;

public class DigestMiddleware(ProxyConfig config) : IMiddleware
{
    public string CalculatePs3Digest(string route, Stream body, string auth, bool isUpload)
    {
        using MemoryStream ms = new();
        
        // If this request is not to an upload endpoint, then we copy the body of the request into the digest data
        if (!isUpload)
        {
            body.CopyTo(ms);
            body.Seek(0, SeekOrigin.Begin);
        }
        
        ms.WriteString(auth);
        ms.WriteString(route);
        ms.WriteString(config.Ps3Digest);

        ms.Position = 0;
        using SHA1 sha = SHA1.Create();
        string digestResponse = Convert.ToHexString(sha.ComputeHash(ms)).ToLower();

        return digestResponse;
    }

    public string CalculatePs4Digest(string route, Stream body, string auth, bool isUpload)
    {
        using MemoryStream ms = new();
        
        // If this request is not to an upload endpoint, then we copy the body of the request into the digest data
        if (!isUpload)
        {
            body.CopyTo(ms);
            body.Seek(0, SeekOrigin.Begin);
        }
        
        ms.WriteString(auth);
        ms.WriteString(route);

        ms.Position = 0;
        
        using HMACSHA1 hmac = new(Encoding.UTF8.GetBytes(config.Ps4Digest));
        string digestResponse = Convert.ToHexString(hmac.ComputeHash(ms)).ToLower();

        return digestResponse;
    }
    
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        string route = context.Uri.AbsolutePath;
        
        //If this isn't an LBP endpoint, dont do digest
        if (!route.StartsWith("/LITTLEBIGPLANETPS3_XML/"))
        {
            next();
            return;
        }
        
        bool isUpload = route.StartsWith("/LITTLEBIGPLANETPS3_XML/upload/");
        string auth = context.Cookies["MM_AUTH"] ?? string.Empty;
        
        // For upload requests, the X-Digest-B header is in use instead by the client
        string digestHeader = isUpload ? "X-Digest-B" : "X-Digest-A";
        string clientDigest = context.RequestHeaders[digestHeader] ?? string.Empty;
            
        // Pass through the client's digest right back to the digest B response
        context.ResponseHeaders["X-Digest-B"] = clientDigest;

        // Run the rest of the middlewares
        next();
        
        // Make sure the calculation reads the whole response stream
        context.ResponseStream.Seek(0, SeekOrigin.Begin);
        
        // If we are a PS4 client, use the PS4 digest code
        if (context.RequestHeaders["User-Agent"] == "MM CHTTPClient LBP3 01.26")
            context.ResponseHeaders["X-Digest-A"] = this.CalculatePs4Digest(route, context.ResponseStream, auth, isUpload);
        // Else, use PS3 digest code
        else
            context.ResponseHeaders["X-Digest-A"] = this.CalculatePs3Digest(route, context.ResponseStream, auth, isUpload);
    }
}