using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Challenges.LbpHub.Ghost;

// Examples of single frames:
// <ghost_frame time="96" X="41" Y="-32"></ghost_frame>
// <ghost_frame time="300" X="-14545" Y="12099" Z="0" rotation="-0.000" keyframe="true"></ghost_frame>

/// <summary>
/// A single frame containing data of the player's position and rotation at a time. The Z coordinate seems to always
/// be a multiple of 50 and related to the current layer. All 3 coordinates seem to be relative to something 
/// (difference from start checkpoint coords? or from the previous frame?). A coordinate or rotation attribute isn't
/// included if it would be 0, however all attributes seem to always be included in keyframes.
/// </summary>
[XmlRoot("ghost_frame")]
[XmlType("ghost_frame")]
public class SerializedChallengeGhostFrame
{
    // Commented out for now because none of these attributes are ever read by us yet,
    // they're only here for documentation purpose.
    /*
    /// <summary>
    /// Starts at 0 and gets incremented with each frame by an unknown amount
    /// </summary>
    [XmlAttribute("time")] public int Time { get; set; }
    [XmlAttribute("X")] public int X { get; set; }
    [XmlAttribute("Y")] public int Y { get; set; }
    [XmlAttribute("Z")] public int Z { get; set; }
    [XmlAttribute("rotation")] public double Rotation { get; set; }
    /// <summary>
    /// True every 100 frames, left out otherwise. 
    /// </summary>
    [XmlAttribute("keyframe")] public bool IsKeyframe { get; set; }
    */
}