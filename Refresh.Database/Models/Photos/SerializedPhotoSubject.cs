using System.Globalization;
using System.Xml.Serialization;

namespace Refresh.Database.Models.Photos;

#nullable disable

[XmlRoot("subject")]
[XmlType("subject")]
public class SerializedPhotoSubject
{
    [XmlElement("npHandle")]
    public string Username { get; set; }
    
    [XmlElement("displayName")]
    public string DisplayName { get; set; }
    
    [XmlElement("bounds")]
    public string BoundsList { get; set; }
    
    public const byte FloatCount = 4;
    
    public static void ParseBoundsList(ReadOnlySpan<char> input, float[] floats)
    {
        byte start = 0;
        byte floatIndex = 0;

        for (byte i = 0; i < input.Length; i++)
        {
            if(floatIndex == FloatCount - 1) break; // won't catch the last float - not worth reading
            if (input[i] != ',') continue;
            
            if (!float.TryParse(input.Slice(start, i - start), NumberFormatInfo.InvariantInfo, out float f))
                throw new FormatException("Invalid format");
                
            floats[floatIndex] = f;
            floatIndex++;
            start = (byte)(i + 1);
        }
        
        // parse the last float value - bounds does not end with comma
        if (!float.TryParse(input[start..], NumberFormatInfo.InvariantInfo, out float lastFloat))
            throw new FormatException("Invalid format");

        floats[FloatCount - 1] = lastFloat;
    }
}