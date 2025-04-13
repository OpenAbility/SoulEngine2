

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

    public static Frustum CreateFromCamera(Vector3 position, Vector3 front, Vector3 right, Vector3 up, float aspect, float fov, float near, float far)
    {
        Frustum frustum = new Frustum();

        float halfVSide = far * MathF.Tan(fov * 0.5f);
        float halfHSide = halfVSide * aspect;

        Vector3 farPoint = front * far;

        frustum.NearFace = new Plane(position + near * front, front);
        frustum.FarFace = new Plane(position + farPoint, -front);
        
        frustum.RightFace = new Plane(position, Vector3.Cross(farPoint - right * halfHSide, up));
        frustum.LeftFace = new Plane(position, Vector3.Cross(up, farPoint + right * halfHSide));

        frustum.TopFace = new Plane(position, Vector3.Cross(right, farPoint - up * halfVSide));
        frustum.BottomFace = new Plane(position, Vector3.Cross(farPoint + up * halfVSide, right));

        return frustum;
    }
    

    public bool InFrustum(Vector3 point)
    {
        return 
               LeftFace.GetDistanceToPlane(point) > 0 &&
               RightFace.GetDistanceToPlane(point) > 0 &&
               NearFace.GetDistanceToPlane(point) > 0 &&
               FarFace.GetDistanceToPlane(point) > 0 &&
               TopFace.GetDistanceToPlane(point) > 0 &&
               BottomFace.GetDistanceToPlane(point) > 0;
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