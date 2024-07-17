using System.Security.Cryptography;
using System.Text;
using Bunkum.Listener.Request;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Refresh.Common.Extensions;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints;

namespace Refresh.GameServer.Middlewares;

public class DigestMiddleware(GameServerConfig config) : IMiddleware
{
    public record PspVersionInfo(short ExeVersion, short DataVersion) {}
    
    public static string CalculateDigest(
        string digest, 
        string route,
        Stream body,
        string auth, 
        PspVersionInfo? pspVersionInfo, 
        bool isUpload, 
        bool hmacDigest)
    {
        // Init a MemoryStream with the known final capacity capacity
        using MemoryStream ms = new((int)(auth.Length + route.Length + digest.Length + (isUpload ? 0 : body.Length)) + (pspVersionInfo == null ? 0 : 4));

        // If this is not an upload endpoint, then we need to copy the body of the request into the digest calculation
        if (!isUpload)
        {
            body.CopyTo(ms);
            body.Seek(0, SeekOrigin.Begin);
        }
        
        ms.WriteString(auth);
        ms.WriteString(route);
        if (pspVersionInfo != null)
        {
            Span<byte> bytes = stackalloc byte[2];
            
            BitConverter.TryWriteBytes(bytes, pspVersionInfo.ExeVersion);
            // If we are on a big endian system, we need to flip the bytes
            if(!BitConverter.IsLittleEndian)
                bytes.Reverse();
            ms.Write(bytes);
            
            BitConverter.TryWriteBytes(bytes, pspVersionInfo.DataVersion);
            // If we are on a big endian system, we need to flip the bytes
            if(!BitConverter.IsLittleEndian)
                bytes.Reverse();
            ms.Write(bytes);
        }
        if(!hmacDigest)
            ms.WriteString(digest);

        ms.Position = 0;

        if (hmacDigest)
        {
            using HMACSHA1 hmac = new(Encoding.UTF8.GetBytes(digest));
            return Convert.ToHexString(hmac.ComputeHash(ms)).ToLower();
        }

        using SHA1 sha = SHA1.Create();
        return Convert.ToHexString(sha.ComputeHash(ms)).ToLower();
    }
    
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        string route = context.Uri.AbsolutePath;
        
        //If this isn't an LBP endpoint, dont do digest
        if (!route.StartsWith(GameEndpointAttribute.BaseRoute) && !route.StartsWith(LegacyAdapterMiddleware.OldBaseRoute))
        {
            next();
            return;
        }

        PspVersionInfo? pspVersionInfo = null;
        // Try to acquire the exe and data version, this is only accounted for in the client digests, not the server digests
        if (short.TryParse(context.RequestHeaders["X-Exe-V"], out short exeVer) &&
            short.TryParse(context.RequestHeaders["X-Data-V"], out short dataVer))
            pspVersionInfo = new PspVersionInfo(exeVer, dataVer);

        string auth = context.Cookies["MM_AUTH"] ?? string.Empty;
        bool isUpload = route.StartsWith($"{LegacyAdapterMiddleware.OldBaseRoute}upload/") || route.StartsWith($"{GameEndpointAttribute.BaseRoute}upload/");

        // For upload requests, the X-Digest-B header is in use instead by the client
        string digestHeader = isUpload ? "X-Digest-B" : "X-Digest-A";
        string clientDigest = context.RequestHeaders[digestHeader] ?? string.Empty;
        
        // Pass through the client's digest right back to the digest B response
        context.ResponseHeaders["X-Digest-B"] = clientDigest;
        
        next();

        GameDatabaseContext gameDatabase = (GameDatabaseContext)database.Value;

        Token? token = gameDatabase.GetTokenFromTokenData(auth, TokenType.Game);
        
        // Make sure the digest calculation reads the whole response stream
        context.ResponseStream.Seek(0, SeekOrigin.Begin);

        // If the digest is already saved on the token, use the token's digest
        if (token is { Digest: not null })
        {
            SetDigestResponse(context, CalculateDigest(token.Digest, route, context.ResponseStream, auth, null, isUpload, token.IsHmacDigest));
            return;
        }

        foreach (string digest in config.Sha1DigestKeys)
        {
            string calculatedClientDigest = CalculateDigest(digest, route, context.InputStream, auth, pspVersionInfo, isUpload, false);

            // If the calculated client digest is invalid, then this isn't the digest the game is using, so check the next one
            if (calculatedClientDigest != clientDigest) 
                continue;

            SetDigestResponse(context, CalculateDigest(digest, route, context.ResponseStream, auth, null, isUpload, false));
            
            if(token != null)
                gameDatabase.SetTokenDigestInfo(token, digest, false);
            
            return;
        }

        foreach (string digest in config.HmacDigestKeys)
        {
            string calculatedClientDigest = CalculateDigest(digest, route, context.InputStream, auth, pspVersionInfo, isUpload, true);

            // If the calculated client digest is invalid, then this isn't the digest the game is using, so check the next one
            if (calculatedClientDigest != clientDigest) 
                continue;
            
            SetDigestResponse(context, CalculateDigest(digest, route, context.ResponseStream, auth, null, isUpload, true));
            
            if(token != null)
                gameDatabase.SetTokenDigestInfo(token, digest, true);
            
            return; 
        }

        // If we were unable to find any digests, just use the first one specified as a backup
        // TODO: once we have PS4 support, check if the token is a PS4 token
        bool isPs4 = context.RequestHeaders["User-Agent"] == "MM CHTTPClient LBP3 01.26";
        string firstDigest = isPs4 ? config.HmacDigestKeys[0] : config.Sha1DigestKeys[0];
        
        SetDigestResponse(context, CalculateDigest(firstDigest, route, context.ResponseStream, auth, null, isUpload, isPs4));
        
        if(token != null)
            gameDatabase.SetTokenDigestInfo(token, firstDigest, isPs4);
    }

    private static void SetDigestResponse(ListenerContext context, string calculatedDigest)
        => context.ResponseHeaders["X-Digest-A"] = calculatedDigest;
}