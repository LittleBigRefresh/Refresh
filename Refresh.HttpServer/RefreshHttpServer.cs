using System.Diagnostics;
using System.Net;
using System.Reflection;
using NotEnoughLogs;
using NotEnoughLogs.Loggers;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Extensions;
using Refresh.HttpServer.Responses;

namespace Refresh.HttpServer;

public class RefreshHttpServer
{
    private readonly HttpListener _listener;
    private readonly List<Endpoint> _endpoints = new();
    private readonly LoggerContainer<HttpContext> _logger;

    public RefreshHttpServer(params string[] listenEndpoints)
    {
        this._logger = new LoggerContainer<HttpContext>();
        this._logger.RegisterLogger(new ConsoleLogger());

        this._listener = new HttpListener();
        this._listener.IgnoreWriteExceptions = true;
        foreach (string endpoint in listenEndpoints)
        {
            this._logger.LogInfo(HttpContext.Startup, "Listening at URI " + endpoint);
            this._listener.Prefixes.Add(endpoint);
        }
    }

    public void Start()
    {
        this._listener.Start();
        Task.Factory.StartNew(async () => await this.Block());
    }
    
    public async Task StartAndBlockAsync()
    {
        this._listener.Start();
        await this.Block();
    }

    private async Task Block()
    {
        while (true)
        {
            HttpListenerContext context = await this._listener.GetContextAsync();

            await Task.Factory.StartNew(() => { this.HandleRequest(context); });
        }
    }

    public void HandleRequest(HttpListenerContext context)
    {
        Stopwatch requestStopwatch = new();
        requestStopwatch.Start();
        
        try
        {
            context.Response.AddHeader("Server", "Refresh");

            string? path = context.Request.Url?.AbsolutePath;

            Response? resp = this._endpoints
                .FirstOrDefault(d => d.Route == path)?
                .GetResponse(context);

            if (resp == null)
            {
                context.Response.AddHeader("Content-Type", ContentType.Plaintext.GetName());
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.WriteString("Not found: " + path);
            }
            else
            {
                context.Response.AddHeader("Content-Type", resp.Value.ContentType.GetName());
                context.Response.StatusCode = (int)resp.Value.StatusCode;
                context.Response.OutputStream.Write(resp.Value.Data);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            try
            {
                context.Response.AddHeader("Content-Type", ContentType.Plaintext.GetName());
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

#if DEBUG
                context.Response.WriteString(e.ToString());
#else
                context.Response.WriteString("Internal Server Error");
#endif
            }
            catch
            {
                // ignored
            }
        }
        finally
        {
            try
            {
                requestStopwatch.Stop();

                this._logger.LogInfo(HttpContext.Request, $"Served request to {context.Request.RemoteEndPoint}: " +
                                                          $"{context.Response.StatusCode} on '{context.Request.Url?.AbsolutePath}' " +
                                                          $"({requestStopwatch.ElapsedMilliseconds}ms)");
                context.Response.Close();
            }
            catch
            {
                // ignored
            }
        }
    }
    
    private void AddEndpoint(Type type)
    {
        Endpoint? doc = (Endpoint?)Activator.CreateInstance(type);
        Debug.Assert(doc != null);
        
        this._endpoints.Add(doc);
    }

    public void AddEndpoint<TDoc>() where TDoc : Endpoint => this.AddEndpoint(typeof(TDoc));

    public void DiscoverEndpointsFromAssembly(Assembly assembly)
    {
        List<Type> types = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Endpoint)))
            .ToList();

        foreach (Type type in types) this.AddEndpoint(type);
    }
}