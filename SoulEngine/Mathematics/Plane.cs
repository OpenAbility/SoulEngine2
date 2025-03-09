using OpenTK.Mathematics;

namespace SoulEngine.Mathematics;

public struct Plane
{
    public Vector3 Normal;
    public float Distance;

    public Plane()
    {
        
    }

    public Plane(Vector3 p1, Vector3 normal)
    {
        Normal = normal.Normalized();
        Distance = Vector3.Dot(Normal, p1);
    }

    public float GetDistanceToPlane(Vector3 point)
    {
        return Vector3.Dot(Normal, point) - Distance;
    }
}