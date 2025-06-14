namespace Refresh.Common.Helpers;

public static class MathHelper
{
    public static float RemapIntToFloat(int input, float min = -1.0f, float max = 1.0f, float inputMax = 512f, float inputMin = -512f)
    {
        return min + (input - inputMin) * (max - min) / (inputMax - inputMin);
    }
}