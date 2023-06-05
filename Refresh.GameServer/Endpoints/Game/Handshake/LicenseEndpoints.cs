using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Configuration;

namespace Refresh.GameServer.Endpoints.Game.Handshake;

public class LicenseEndpoints : EndpointGroup
{
    private const string AGPLLicense = """
    Copyright (C) 2023 LittleBigRefresh

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
    """;
    
    [GameEndpoint("eula")]
    public string License(RequestContext context, GameServerConfig config)
    {
        return config.LicenseText + "\n\n" + AGPLLicense + "\n";
    }

    [GameEndpoint("announce")]
    public string Announce(RequestContext context, GameServerConfig config)
    {
        return config.AnnounceText;
    }
}