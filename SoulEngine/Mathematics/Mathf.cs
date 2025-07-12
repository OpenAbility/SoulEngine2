using OpenTK.Mathematics;

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

    public static Quaternion BetweenVectors(Vector3 v1, Vector3 v2)
    {

        v1 = v1.Normalized();
        v2 = v2.Normalized();
		
        Quaternion q = Quaternion.Identity;
        Vector3 a = Vector3.Cross(v1, v2);
        q.Xyz = a;
        q.W = Single.Sqrt((v1.LengthSquared) * (v2.LengthSquared)) + Vector3.Dot(v1, v2);
        q.Normalize();
        return q;
    }

    public static Vector3 Swizzle(Vector3 x, Vector3 y, Vector3 z)
    {
        return new Vector3(x.X, y.Y, z.Z);
    }
}