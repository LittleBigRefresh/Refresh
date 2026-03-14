using System.Globalization;

namespace Refresh.Database.Helpers;

public abstract class PhotoHelper
{
    public const byte SubjectBoundaryCount = 4;
    
    public static float[] ParseBoundsList(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException($"Bounds are empty");

        string[] boundsStr = input.Split(',');
        if (boundsStr.Length != SubjectBoundaryCount)
            throw new ArgumentException($"Number of bounds is invalid ({boundsStr.Length} instead of {SubjectBoundaryCount})");

        float[] boundsParsed = new float[SubjectBoundaryCount];
        for (int i = 0; i < SubjectBoundaryCount; i++)
        {
            // No OutOfRangeExceptions as both arrays are ensured to be as long as the constant
            string boundaryStr = boundsStr[i];

            if (!float.TryParse(boundaryStr, NumberFormatInfo.InvariantInfo, out float f))
                throw new FormatException($"Boundary '{boundaryStr}' ({i+1}/{SubjectBoundaryCount}) is not a float");

            boundsParsed[i] = f;
        }

        return boundsParsed;
    }
}