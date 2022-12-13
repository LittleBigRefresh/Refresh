using System.Diagnostics;
using System.Net;
using System.Reflection;
using JetBrains.Annotations;
using NotEnoughLogs;
using NotEnoughLogs.Loggers;
using Refresh.HttpServer.Authentication;
using Refresh.HttpServer.Authentication.Dummy;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Extensions;
using Refresh.HttpServer.Responses;

namespace Refresh.HttpServer;

public class RefreshHttpServer
{
    private readonly HttpListener _listener;
    private readonly List<EndpointGroup> _endpoints = new();
    private readonly LoggerContainer<HttpContext> _logger;
    private IAuthenticationProvider<IUser> _authenticationProvider = new DummyAuthenticationProvider();

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
        if (this._authenticationProvider is DummyAuthenticationProvider)
        {
            this._logger.LogWarning(HttpContext.Startup, "The server was started with a dummy authentication provider. " +
                                                         "If your endpoints rely on authentication, users will always have full access.");
        }
        
        while (true)
        {
            HttpListenerContext context = await this._listener.GetContextAsync();

            await Task.Factory.StartNew(() => { this.HandleRequest(context); });
        }
    }

    [Pure]
    private Response? InvokeEndpointByRequest(HttpListenerContext context)
    {
        foreach (EndpointGroup group in this._endpoints)
        {
            foreach (MethodInfo method in group.GetType().GetMethods())
            {
                EndpointAttribute? attribute = method.GetCustomAttribute<EndpointAttribute>();
                if(attribute == null) continue;

                IUser? user = null;
                if (method.GetCustomAttribute<RequiresAuthenticationAttribute>() != null)
                {
                    user = this._authenticationProvider.AuthenticateUser(context.Request);
                    if (user == null)
                        return new Response(Array.Empty<byte>(), ContentType.Plaintext, HttpStatusCode.Forbidden);
                }

                // TODO: check http method
                if (attribute.Route != context.Request.Url!.AbsolutePath) continue;

                List<object?> invokeList = new() { context };
                if(user != null) invokeList.Add(user);

                object? val = method.Invoke(group, invokeList.ToArray());
                
                switch (val)
                {
                    case null:
                        return new Response(Array.Empty<byte>(), attribute.ContentType, HttpStatusCode.NoContent);
                    case Response response:
                        return response;
                    default:
                        return new Response(val, attribute.ContentType);
                }
            }
        }

        return null;
    }

    private void HandleRequest(HttpListenerContext context)
    {
        Stopwatch requestStopwatch = new();
        requestStopwatch.Start();
        
        try
        {
            context.Response.AddHeader("Server", "Refresh");

            string path = context.Request.Url!.AbsolutePath;

            Response? resp = this.InvokeEndpointByRequest(context);

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
    
    private void AddEndpointGroup(Type type)
    {
        EndpointGroup? doc = (EndpointGroup?)Activator.CreateInstance(type);
        Debug.Assert(doc != null);
        
        this._endpoints.Add(doc);
    }

    public void AddEndpointGroup<TDoc>() where TDoc : EndpointGroup => this.AddEndpointGroup(typeof(TDoc));

    public void DiscoverEndpointsFromAssembly(Assembly assembly)
    {
        List<Type> types = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(EndpointGroup)))
            .ToList();

        foreach (Type type in types) this.AddEndpointGroup(type);
    }

    public void UseAuthenticationProvider(IAuthenticationProvider<IUser> provider)
    {
        this._authenticationProvider = provider;
    }
}