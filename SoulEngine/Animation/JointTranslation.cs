using OpenTK.Mathematics;
using SoulEngine.Models;

namespace SoulEngine.Animation;

internal struct JointTranslation(SkeletonJointData jointData)
{
    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    public bool HasPosition;
    public bool HasRotation;
    public bool HasScale;
        
    public readonly SkeletonJointData JointData = jointData;
        
    public Matrix4 Matrix => Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) *
                             Matrix4.CreateTranslation(Position);

    public void Reset()
    {
        Position = JointData.DefaultPosition;
        Rotation = JointData.DefaultRotation;
        Scale = JointData.DefaultScale;
    }
}