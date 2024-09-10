using Bunkum.Core;
using NPTicket;
using Refresh.GameServer.Authentication;

namespace Refresh.GameServer.Extensions;

public static class TicketExtensions
{
    public static TokenPlatform? DeterminePlatform(this Ticket ticket)
    {
        if (ticket.SignatureIdentifier == "RPCN" || ticket.IssuerId == 0x33333333)
            return TokenPlatform.RPCS3;

        if (ticket.IssuerId == 0x100)
            return TokenPlatform.PS3;

        return null;
    }

    public static TokenGame? DetermineGame(this Ticket ticket, RequestContext context)
    {
        TokenGame? game = null;

        // check if we're connecting from a beta build
        bool parsedBeta = byte.TryParse(context.QueryString.Get("beta"), out byte isBeta);
        if (parsedBeta && isBeta == 1) game = TokenGame.BetaBuild;

        game ??= TokenGameUtility.FromTitleId(ticket.TitleId);
        return game;
    }
}