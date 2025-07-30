using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Bunkum.Listener.Request;
using Org.BouncyCastle.Utilities.Zlib;
using Refresh.Core.Configuration;
using Refresh.Interfaces.Game;

namespace Refresh.GameServer.Middlewares;

/// <summary>
/// Adds deflate encoding to LBP game server responses if they reach a certain size.
/// The game will eventually corrupt its own memory if we do not do this,
/// since non-deflate requests only get a very small request buffer, and if you send too big of a request,
/// it overruns that buffer and starts corrupting random heap memory.
/// Requests encoded with deflate get a much larger read buffer, so they avoid this problem until much larger buffer sizes.
/// </summary>
public class DeflateMiddleware(GameServerConfig config) : IMiddleware
{
    /// <summary>
    /// After this many bytes, start deflating request bodies
    /// </summary>
    private const int DeflateCutoff = 1024;
    
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        next();
        
        if (!config.UseDeflateCompression)
            return;
        
        // If this isn't a game request, it isn't necessary to do anything here.
        if (!context.Uri.AbsolutePath.StartsWith(GameEndpointAttribute.BaseRoute))
            return;
        
        // If the request is coming through Cloudflare, tell cloudflare not to touch the data in any way
        if (context.RequestHeaders["Cf-Ray"] != null)
            context.ResponseHeaders["Cache-Control"] = "no-transform";
        
        // If this is a resource request, don't deflate because resources will be corrupted if deflated.
        if (context.Uri.AbsolutePath.StartsWith($"{GameEndpointAttribute.BaseRoute}r/"))
            return;
        
        string? encodings = context.RequestHeaders.Get("Accept-Encoding");
        // If the accepted encodings aren't specified, or they don't contain deflate/gzip, or we don't need to use deflate on the data, do nothing.
        if (encodings == null || (!encodings.Contains("deflate") && !encodings.Contains("gzip")) || context.ResponseStream.Length <= DeflateCutoff) 
            return;

        // Update the headers marking that we are sending encoded data
        context.ResponseHeaders["X-Original-Content-Length"] = context.ResponseStream.Length.ToString();
        context.ResponseHeaders["Vary"] = "Accept-Encoding";
        context.ResponseHeaders["Content-Encoding"] = "deflate";
        
        // Create a copy of our uncompressed data
        byte[] uncompressed = context.ResponseStream.ToArray();
        
        // Reset the response stream position and length to 0, so we can start writing to it again
        context.ResponseStream.Position = 0;
        context.ResponseStream.SetLength(0);
        
        // Compress our source data into the response stream
        using ZOutputStreamLeaveOpen zlibStream = new(context.ResponseStream, 6);
        zlibStream.Write(uncompressed);
        zlibStream.Finish();
        zlibStream.Close();
        
        // Seek back to the start
        context.ResponseStream.Position = 0;
    }
}