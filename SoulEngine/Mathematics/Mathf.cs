namespace SoulEngine.Mathematics;

public static class Mathf
{
    public static float Deg2Rad = MathF.PI / 180;
    public static float Rag2Deg = 1 / Deg2Rad;
    
    
    public static float LerpUnclamped(float from, float to, float delta)
    {
        return from + (to - from) * delta;
    }
    
    public static float Lerp(float from, float to, float delta)
    {
        return Lerp(from, to, Clamp(delta, 0, 1));
    }

    public static float Clamp(float value, float min, float max)
    {
        return Math.Min(max, Math.Max(value, min));
    }
}