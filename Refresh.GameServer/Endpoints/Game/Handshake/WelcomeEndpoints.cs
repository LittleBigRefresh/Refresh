using System.Text;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Types.Roles;

namespace Refresh.GameServer.Endpoints.Game.Handshake;

public class WelcomeEndpoints : EndpointGroup
{
    private const string Copyright = "Copyright (C) 2024 LittleBigRefresh";
    private const string AGPLNotice = """
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
    [MinimumRole(GameUserRole.Restricted)]
    public string License(RequestContext context, GameServerConfig config, ContactInfoConfig contact)
    {
        StringBuilder builder = new();
        
        builder.AppendLine(config.LicenseText);

        builder.AppendLine();
        builder.AppendLine($"{config.InstanceName} is operated by {contact.AdminName}, who you can email at {contact.EmailAddress}.");
        if (contact.AdminDiscordUsername != null)
            builder.AppendLine($"Alternatively, you can also contact {contact.AdminDiscordUsername} on Discord.");

        if (contact.DiscordServerInvite != null)
        {
            builder.AppendLine();
            builder.AppendLine($"We have a Discord server! Feel free to join here: {contact.DiscordServerInvite}");
        }

        builder.AppendLine(new string('=', Copyright.Length));
        builder.AppendLine(Copyright);
        builder.AppendLine();
        builder.AppendLine(AGPLNotice);

        return builder.ToString();
    }
}