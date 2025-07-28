using OpenTK.Mathematics;
using SoulEngine.Content;
using SoulEngine.Data.NBT;
using SoulEngine.Resources;
using SoulEngine.Util;

namespace SoulEngine.Models;

// Basically, a skeleton just provides generic data, and the instance
// tells us how shit is done.

[Resource("e.skele", typeof(SkeletonLoader))]
[ExpectedExtensions(".skeleton")]
public class Skeleton : Resource
{
    private List<SkeletonJointData> joints = new List<SkeletonJointData>();
    private Dictionary<string, SkeletonJointData> namedJoints = new Dictionary<string, SkeletonJointData>();

    private SkeletonJointData CreateJoint(string name, SkeletonJointData? parent, Matrix4 inverseBind, Matrix4 defaultMatrix)
    {
        SkeletonJointData jointData = new SkeletonJointData(joints.Count, name, parent, inverseBind, defaultMatrix);
        joints.Add(jointData);
        namedJoints[name] = jointData;
        return jointData;
    }

    public int JointCount => joints.Count;

    public SkeletonJointData? GetJoint(string name)
    {
        return namedJoints.GetValueOrDefault(name);
    }
    
    public SkeletonJointData GetJoint(int id)
    {
        return joints[id];
    }
    
    public SkeletonInstance Instantiate()
    {
        return new SkeletonInstance(this);
    }
    
    private class SkeletonLoader : IResourceLoader<Skeleton>
    {

        private SkeletonJointData LoadJoint(Skeleton skeleton, CompoundTag tag, SkeletonJointData? parent)
        {
            SkeletonJointData jointData = skeleton.CreateJoint(
                tag.GetString("name")!, 
                parent,
                tag.GetTag<ListTag>("inverseMatrix")!.ToMatrix(),
                tag.GetTag<ListTag>("matrix")!.ToMatrix()
                );

            foreach (var child in tag.GetTag<ListTag>("children")!)
            {
                LoadJoint(skeleton, (CompoundTag)child, jointData);
            }
            
            return jointData;
        }
        
        public Skeleton LoadResource(ResourceData data)
        {
            if (data.ResourcePath == "skele/misc.skeleton")
            {
                Skeleton misc = new Skeleton();

                misc.CreateJoint("root", null, Matrix4.Identity, Matrix4.Identity);
                
                return misc;
            }
            
            CompoundTag? rootTag = TagIO.ReadCompressed(data.ResourceStream, false) as CompoundTag;

            if (rootTag == null)
                throw new Exception("Root skeleton tag is not compound!");
            
            Skeleton skeleton = new Skeleton();

            LoadJoint(skeleton, rootTag.GetTag<CompoundTag>("joints")!, null);

            return skeleton;
        }
    }
}

/// <summary>
/// Data shared between all instances of a joint
/// </summary>
public class SkeletonJointData
{
    public readonly int SkeletonID;
    public readonly string Name;
    public readonly SkeletonJointData? Parent;

    public readonly Matrix4 InverseBind;
    public readonly Matrix4 DefaultMatrix;

    public readonly Vector3 DefaultPosition;
    public readonly Vector3 DefaultScale;
    public readonly Quaternion DefaultRotation;

    public SkeletonJointData(int skeletonId, string name, SkeletonJointData? parent, Matrix4 inverseBind, Matrix4 defaultMatrix)
    {
        SkeletonID = skeletonId;
        Name = name;
        Parent = parent;
        InverseBind = inverseBind;
        DefaultMatrix = defaultMatrix;

        DefaultPosition = defaultMatrix.ExtractTranslation();
        DefaultScale = defaultMatrix.ExtractScale();
        DefaultRotation = defaultMatrix.ExtractRotation();
    }
}