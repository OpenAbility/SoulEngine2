using OpenTK.Mathematics;

namespace SoulEngine.Mathematics;

public static class Mathx
{
    public static float Deg2Rad = MathF.PI / 180;
    public static float Rag2Deg = 1 / Deg2Rad;
    
    
    public static float LerpUnclamped(float from, float to, float delta)
    {
        return from + (to - from) * delta;
    }
    
    public static float Lerp(float from, float to, float delta)
    {
        return LerpUnclamped(from, to, Clamp(delta, 0, 1));
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

    public static System.Numerics.Vector3 System(this Vector3 vector)
    {
        return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
    }

    public static Vector3 Tk(this System.Numerics.Vector3 vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }
    
    public static System.Numerics.Vector2 System(this Vector2 vector)
    {
        return new System.Numerics.Vector2(vector.X, vector.Y);
    }

    public static Vector2 Tk(this System.Numerics.Vector2 vector)
    {
        return new Vector2(vector.X, vector.Y);
    }
}