using System.Security.Cryptography;
using System.Text;
using Bunkum.Listener.Request;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Refresh.Common.Extensions;
using Refresh.Common.Helpers;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints;

namespace Refresh.GameServer.Middlewares;

public class DigestMiddleware : IMiddleware
{
    private readonly GameServerConfig _config;

    public DigestMiddleware(GameServerConfig config)
    {
        this._config = config;
    }

    public record PspVersionInfo(short ExeVersion, short DataVersion) {}
    
    public static string CalculateDigest(
        string digest, 
        string route,
        ReadOnlySpan<byte> body,
        string auth, 
        PspVersionInfo? pspVersionInfo, 
        bool isUpload, 
        bool hmacDigest)
    {
        IncrementalHash hash = hmacDigest
            ? IncrementalHash.CreateHMAC(HashAlgorithmName.SHA1, Encoding.UTF8.GetBytes(digest))
            : IncrementalHash.CreateHash(HashAlgorithmName.SHA1);

        // If this is not an upload endpoint, then we need to copy the body of the request into the digest calculation
        if (!isUpload)
        {
            hash.AppendData(body);
        }

        hash.WriteString(auth);
        hash.WriteString(route);
        if (pspVersionInfo != null)
        {
            Span<byte> bytes = stackalloc byte[2];
            
            BitConverter.TryWriteBytes(bytes, pspVersionInfo.ExeVersion);
            // If we are on a big endian system, we need to flip the bytes
            if(!BitConverter.IsLittleEndian)
                bytes.Reverse();
            hash.AppendData(bytes);
            
            BitConverter.TryWriteBytes(bytes, pspVersionInfo.DataVersion);
            // If we are on a big endian system, we need to flip the bytes
            if(!BitConverter.IsLittleEndian)
                bytes.Reverse();
            hash.AppendData(bytes);
        }
        
        if(!hmacDigest)
            hash.WriteString(digest);

        return HexHelper.BytesToHexString(hash.GetCurrentHash());
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

        // TODO: once we have PS4 support, check if the token is a PS4 token
        bool isPs4 = context.RequestHeaders["User-Agent"]?.Contains("MM CHTTPClient LBP3 01.26") ?? false;
        
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
        
        byte[] responseBody = context.ResponseStream.ToArray();
        byte[] requestBody = context.InputStream.ToArray();
        
        // Make sure the digest calculation reads the whole response stream
        context.ResponseStream.Seek(0, SeekOrigin.Begin);

        // If the digest is already saved on the token, use the token's digest
        if (token is { Digest: not null })
        {
            SetDigestResponse(context, CalculateDigest(token.Digest, route, responseBody, auth, null, isUpload, token.IsHmacDigest));
            return;
        }

        // If the client asks for a particular digest index, use that digest
        if (int.TryParse(context.RequestHeaders["Refresh-Ps3-Digest-Index"], out int ps3DigestIndex) && 
            int.TryParse(context.RequestHeaders["Refresh-Ps4-Digest-Index"], out int ps4DigestIndex))
        {
            string digest = isPs4
                ? this._config.HmacDigestKeys[ps4DigestIndex]
                : this._config.Sha1DigestKeys[ps3DigestIndex];
            
            SetDigestResponse(context, CalculateDigest(digest, route, responseBody, auth, null, isUpload, isPs4));
            
            if(token != null)
                gameDatabase.SetTokenDigestInfo(token, digest, isPs4);

            return;
        }

        (string digest, bool hmac)? foundDigest = this.FindBestKey(clientDigest, route, requestBody, auth, pspVersionInfo, isUpload, false) ??
                                                  this.FindBestKey(clientDigest, route, requestBody, auth, pspVersionInfo, isUpload, true);

        if (foundDigest != null)
        {
            string digest = foundDigest.Value.digest;
            bool hmac = foundDigest.Value.hmac;
            
            SetDigestResponse(context, CalculateDigest(digest, route, responseBody, auth, null, isUpload, hmac));
        
            if(token != null)
                gameDatabase.SetTokenDigestInfo(token, digest, hmac); 
        }
        else
        {
            // If we were unable to find any digests, just use the first one specified as a backup
            string firstDigest = isPs4 ? this._config.HmacDigestKeys[0] : this._config.Sha1DigestKeys[0];
        
            SetDigestResponse(context, CalculateDigest(firstDigest, route, responseBody, auth, null, isUpload, isPs4));
        
            // If theres no token, or the client didnt provide any client digest, lock the token into the found digest
            // The second condition is to make sure that we dont lock in a digest for endpoints which send no client digest,
            // but expect a server digest.
            if(token != null && !string.IsNullOrEmpty(clientDigest))
                gameDatabase.SetTokenDigestInfo(token, firstDigest, isPs4);
        }
    }

    private (string digest, bool hmac)? FindBestKey(string clientDigest, string route, ReadOnlySpan<byte> requestData, string auth, PspVersionInfo? pspVersionInfo, bool isUpload, bool hmac)
    {
        string[] keys = hmac ? this._config.HmacDigestKeys : this._config.Sha1DigestKeys;
        
        foreach (string digest in keys)
        {
            string calculatedClientDigest = CalculateDigest(digest, route, requestData, auth, pspVersionInfo, isUpload, hmac);

            // If the calculated client digest is invalid, then this isn't the digest the game is using, so check the next one
            if (calculatedClientDigest != clientDigest) 
                continue;

            // If they match, we found the client's digest
            return (digest, hmac);
        }

        return null;
    }

    private static void SetDigestResponse(ListenerContext context, string calculatedDigest)
        => context.ResponseHeaders["X-Digest-A"] = calculatedDigest;
}