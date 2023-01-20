using System.ComponentModel;
using Refresh.HttpServer.Configuration;

namespace Refresh.GameServer.Configuration;

public interface IGameServerConfig : IConfig
{
    [DefaultValue("This is the announce text. You can change the value in the configuration file!")]
    public string AnnounceText { get; }
    
    [DefaultValue("Welcome to Refresh!")]
    public string LicenseText { get; }
    
    // TODO: Move to IConfig when .NET 8 is released (https://github.com/dotnet/runtime/pull/78788)
    [DefaultValue("http://127.0.0.1:10061")]
    public string ExternalUrl { get; }
}