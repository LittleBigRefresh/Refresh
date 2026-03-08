using System.Globalization;

namespace Refresh.Database.Helpers;

public abstract class PhotoHelper
{
    public const byte SubjectBoundCount = 4;
    
    public static void ParseBoundsList(ReadOnlySpan<char> input, float[] floats)
    {
        byte start = 0;
        byte floatIndex = 0;

        for (byte i = 0; i < input.Length; i++)
        {
            if(floatIndex == SubjectBoundCount - 1) break; // won't catch the last float - not worth reading
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

        floats[SubjectBoundCount - 1] = lastFloat;
    }
}