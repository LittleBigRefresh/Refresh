using System.ComponentModel;
using Refresh.HttpServer.Configuration;

namespace Refresh.GameServer.Configuration;

public interface IGameServerConfig : IConfig
{
    [DefaultValue("This is the announce text. You can change the value in the configuration file!")]
    public string AnnounceText { get; }
    
    [DefaultValue("Welcome to Refresh!")]
    public string LicenseText { get; }
}