using System.Diagnostics;
using System.Security.Cryptography;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Endpoints.Middlewares;
using Refresh.GameServer.Extensions;

namespace Refresh.GameServer.Middlewares;

public class DigestMiddleware : IMiddleware
{
    // Should be 19 characters (or less maybe?)
    // Length was taken from PS3 and PS4 digest keys
    private const string DigestKey = "CustomServerDigest";

    public static string CalculateDigest(string url, Stream body, string auth)
    {
        using MemoryStream ms = new();
        
        if (!url.StartsWith("/lbp/upload/"))
        {
            // get request body
            body.CopyTo(ms);
            body.Seek(0, SeekOrigin.Begin);
        }

        ms.WriteString(auth);
        ms.WriteString(url);
        ms.WriteString(DigestKey);

        ms.Position = 0;
        using SHA1 sha = SHA1.Create();
        string digestResponse = Convert.ToHexString(sha.ComputeHash(ms)).ToLower();

        return digestResponse;
    }
    
    // Referenced from Project Lighthouse
    // https://github.com/LBPUnion/ProjectLighthouse/blob/d16132f67f82555ef636c0dabab5aabf36f57556/ProjectLighthouse.Servers.GameServer/Middlewares/DigestMiddleware.cs
    // https://github.com/LBPUnion/ProjectLighthouse/blob/19ea44e0e2ff5f2ebae8d9dfbaf0f979720bd7d9/ProjectLighthouse/Helpers/CryptoHelper.cs#L35
    private bool VerifyDigestRequest(ListenerContext context)
    {
        string url = context.Uri.AbsolutePath;
        string auth = context.Cookies["MM_AUTH"] ?? string.Empty;
    
        string digestResponse = CalculateDigest(url, context.InputStream, auth);
    
        string digestHeader = !url.StartsWith("/lbp/upload/") ? "X-Digest-A" : "X-Digest-B";
        string clientDigest = context.RequestHeaders[digestHeader] ?? string.Empty;
        
        context.ResponseHeaders["X-Digest-B"] = digestResponse;
        if (clientDigest == digestResponse) return true;
        
        // this._logger.LogWarning(BunkumContext.Digest, $"Digest failed: {clientDigest} != {digestResponse}");
        return false;
    }
    
    private void SetDigestResponse(ListenerContext context)
    {
        string url = context.Uri.AbsolutePath;
        string auth = context.Cookies["MM_AUTH"] ?? string.Empty;
    
        string digestResponse = CalculateDigest(url, context.ResponseStream, auth);
        
        context.ResponseHeaders["X-Digest-A"] = digestResponse;
    }

    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        if (!context.Uri.AbsolutePath.StartsWith("/lbp"))
        {
            next();
            return;
        }
        
        this.VerifyDigestRequest(context);
        Debug.Assert(context.InputStream.Position == 0); // should be at position 0 before we pass down the pipeline
        
        next();
        
        this.SetDigestResponse(context);
    }
}