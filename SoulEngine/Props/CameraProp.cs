using OpenTK.Mathematics;
using SoulEngine.Core;

namespace SoulEngine.Props;

[Prop("camera")]
public class CameraProp : Prop
{

    public Vector3 Forward => RotationQuat * -Vector3.UnitZ;
    public Vector3 Up => RotationQuat * Vector3.UnitY;
    public Vector3 Right => RotationQuat * Vector3.UnitX;
    
    public CameraProp(Scene scene, string type, string name) : base(scene, type, name)
    {
    }

    public Matrix4 GetView()
    {
        return Matrix4.LookAt(Position, Position + Forward, Vector3.UnitY);
    }

    public Matrix4 GetProjection(float aspect)
    {
        return Matrix4.CreatePerspectiveFieldOfView(60 * MathF.PI / 180f, aspect, 0.1f, 1000f);
    }
}