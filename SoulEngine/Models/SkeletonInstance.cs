using OpenTK.Mathematics;

namespace SoulEngine.Models;

public class SkeletonInstance
{
    public readonly Skeleton Skeleton;

    private readonly Matrix4[] Matrices;
    
    public SkeletonInstance(Skeleton skeleton)
    {
        Skeleton = skeleton;
        Matrices = new Matrix4[skeleton.JointCount];

        for (int i = 0; i < skeleton.JointCount; i++)
        {
            Matrices[i] = skeleton.GetJoint(i).DefaultMatrix;
        }
    }

    public Matrix4 GetJointLocalMatrix(SkeletonJointData jointData)
    {
        return Matrices[jointData.SkeletonID];
    }
    
    public Matrix4 GetJointGlobalMatrix(SkeletonJointData jointData)
    {
        return Matrices[jointData.SkeletonID] * (jointData.Parent == null ? Matrix4.Identity : GetJointGlobalMatrix(jointData.Parent));
    }

    public void TranslateJoint(SkeletonJointData jointData, Matrix4 position)
    {
        Matrices[jointData.SkeletonID] = position;
    }
    
}