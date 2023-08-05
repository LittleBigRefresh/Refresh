using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Refresh.GameServer.Types.Matching.Responses;

#nullable disable

/*
 *  "Players":[
 *      {"PlayerId":"ppjose7","matching_res":0}
 *      ,{"PlayerId":"ChainlessFreedom","matching_res":0}
 *      ,{"PlayerId":"v-still","matching_res":0}
 *      ,{"PlayerId":"shanzenos","matching_res":1}
 *  ]
 */

[JsonObject(MemberSerialization.OptIn)]
public class SerializedRoomPlayer
{
    [ExcludeFromCodeCoverage]
    public SerializedRoomPlayer() {}

    public SerializedRoomPlayer(string username, byte matchingRes)
    {
        this.Username = username;
        this.MatchingRes = matchingRes;
    }

    // Yes, the casing for this object is inconsistent. Media Molecule is driving me insane. 
    [JsonProperty("PlayerId")] public string Username { get; set; }
    /// <summary>
    /// 0 when it's a player in a different room, 1 when it's you or another player in your room. I think?
    /// </summary>
    [JsonProperty("matching_res")] public byte MatchingRes { get; set; }
}