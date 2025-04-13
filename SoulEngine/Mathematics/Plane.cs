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

    public static Vector3 Intersection(Plane p1, Plane p2, Plane p3)
    {
        Matrix3 M = new Matrix3(p1.Normal, p2.Normal, p3.Normal);

        if (M.Determinant == 0)
            throw new InvalidOperationException("Planes do not intersect at a single point.");

        Vector3 d = new Vector3(p1.Distance, p2.Distance, p3.Distance);
        return M.Inverted() * d;
    }
}