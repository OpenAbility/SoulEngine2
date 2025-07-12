

using OpenTK.Mathematics;

namespace SoulEngine.Mathematics;

public struct Frustum
{

    public Plane TopFace;
    public Plane BottomFace;

    public Plane RightFace;
    public Plane LeftFace;

    public Plane FarFace;
    public Plane NearFace;

    public static Frustum CreateFromCamera(Vector3 position, Vector3 front, Vector3 up, float aspect, float fov, float near, float far)
    {
        Frustum frustum = new Frustum();

        fov *= Mathf.Deg2Rad;

        Vector3 right = Vector3.Cross(front, up).Normalized();
        up = Vector3.Cross(front, right).Normalized();

        float halfVSide = far * MathF.Tan(fov * 0.5f);
        float halfHSide = halfVSide * aspect;

        Vector3 farPoint = front * far;

        frustum.NearFace = new Plane(position + near * front, front);
        frustum.FarFace = new Plane(position + farPoint, -front);
        
        frustum.LeftFace = new Plane(position, -Vector3.Cross(farPoint - right * halfHSide, up));
        frustum.RightFace = new Plane(position, -Vector3.Cross(up, farPoint + right * halfHSide));

        frustum.TopFace = new Plane(position, -Vector3.Cross(right, farPoint - up * halfVSide));
        frustum.BottomFace = new Plane(position, -Vector3.Cross(farPoint + up * halfVSide, right));

        return frustum;
    }

    public static Vector3[] CreatePointsFromCamera(Vector3 position, Vector3 front, Vector3 up, float aspect, float fov, float near, float far)
    {
        fov *= Mathf.Deg2Rad;
        
        Vector3 right = Vector3.Cross(front, up).Normalized();
        up = Vector3.Cross(front, right).Normalized();
        
        float farHeight = far * MathF.Tan(fov * 0.5f);
        float farWidth = farHeight * aspect;
        
        float nearHeight = near * MathF.Tan(fov * 0.5f);
        float nearWidth = nearHeight * aspect;
        

        return
        [
            position + near * front + right * nearWidth + up * nearHeight,
            position + near * front - right * nearWidth + up * nearHeight,
            
            position + near * front + right * nearWidth - up * nearHeight,
            position + near * front - right * nearWidth - up * nearHeight,
            
            position + far * front + right * farWidth + up * farHeight,
            position + far * front - right * farWidth + up * farHeight,
            
            position + far * front + right * farWidth - up * farHeight,
            position + far * front - right * farWidth - up * farHeight
        ];

    }
    

    public bool InFrustum(Vector3 point)
    {
        return NearFace.GetDistanceToPlane(point) > 0 && 
               FarFace.GetDistanceToPlane(point) > 0 &&
               LeftFace.GetDistanceToPlane(point) > 0 && 
               RightFace.GetDistanceToPlane(point) > 0 && 
               BottomFace.GetDistanceToPlane(point) > 0 && 
               TopFace.GetDistanceToPlane(point) > 0;
    }

    public bool InFrustum(AABB aabb)
    {
        return InFrustum(Mathf.Swizzle(aabb.Min, aabb.Min, aabb.Max)) ||
               InFrustum(Mathf.Swizzle(aabb.Min, aabb.Max, aabb.Min)) ||
               InFrustum(Mathf.Swizzle(aabb.Min, aabb.Max, aabb.Max)) ||
               InFrustum(Mathf.Swizzle(aabb.Max, aabb.Min, aabb.Min)) ||
               InFrustum(Mathf.Swizzle(aabb.Max, aabb.Min, aabb.Max)) ||
               InFrustum(Mathf.Swizzle(aabb.Max, aabb.Max, aabb.Min)) ||
               InFrustum(aabb.Min) || InFrustum(aabb.Max);
    }
    
    public Vector3[] GetCorners()
    {
        return
        [
            // Near Plane Corners
            Plane.Intersection(NearFace, TopFace, LeftFace),   // Near-Top-Left
            Plane.Intersection(NearFace, TopFace, RightFace),  // Near-Top-Right
            Plane.Intersection(NearFace, BottomFace, LeftFace),// Near-Bottom-Left
            Plane.Intersection(NearFace, BottomFace, RightFace),// Near-Bottom-Right

            // Far Plane Corners
            Plane.Intersection(FarFace, TopFace, LeftFace),   // Far-Top-Left
            Plane.Intersection(FarFace, TopFace, RightFace),  // Far-Top-Right
            Plane.Intersection(FarFace, BottomFace, LeftFace),// Far-Bottom-Left
            Plane.Intersection(FarFace, BottomFace, RightFace)// Far-Bottom-Right
        ];
    }

}