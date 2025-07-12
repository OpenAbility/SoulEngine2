using System.Text;
using OpenAbility.Logging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Animation;
using SoulEngine.Content;
using SoulEngine.Core;
using SoulEngine.Mathematics;
using SoulEngine.Rendering;
using SoulEngine.Resources;
using SoulEngine.Util;
using Material = SoulEngine.Resources.Material;

namespace SoulEngine.Models;

[Resource("e.mdl", typeof(Loader))]
[ExpectedExtensions(".mdl")]
public class Model : Resource
{

    public MeshData[] Meshes { get; private set; } = [];
    public Skeleton? Skeleton { get; private set; }
    
    public AABB BoundingBox { get; private set; }

    public readonly Dictionary<int, int> SkeletonToMeshJoints = new Dictionary<int, int>();
    public readonly Dictionary<int, AABB> MeshJointBoundingBoxes = new Dictionary<int, AABB>();
    public readonly Dictionary<string, AnimationClip> Animations = new Dictionary<string, AnimationClip>();

    private readonly Game game;
    public Model(Game game)
    {
        this.game = game;
    }
    
    private unsafe void Load(Stream stream)
    {
        BinaryReader reader = new BinaryReader(stream);

        if (Encoding.UTF8.GetString(reader.ReadBytes(4)) != "MODL")
            throw new Exception("File is not SoulEngine mdl!");

        bool hasSkeleton = reader.ReadBoolean();
        if (hasSkeleton)
        {
            string skeletonPath = reader.ReadString();
            Skeleton = game.ResourceManager.Load<Skeleton>(skeletonPath);
            
            int skinJointCount = reader.ReadInt32();
        
            for (int i = 0; i < skinJointCount; i++)
            {
                string name = reader.ReadString();
                int index = reader.ReadInt32();

                SkeletonJointData? data = Skeleton.GetJoint(name);
                if(data != null)
                    SkeletonToMeshJoints[data.SkeletonID] = index;
            }
        }
        
        int meshCount = reader.ReadInt32();
        Meshes = new MeshData[meshCount];

        for (int i = 0; i < meshCount; i++)
        {
            string material = reader.ReadString();
            
            int totalVertices = reader.ReadInt32();
            int totalIndices = reader.ReadInt32();

            Mesh mesh = new Mesh(game);

            MeshBuildData meshBuildData =
                game.ThreadSafety.EnsureMain(() => mesh.BeginUpdate(totalVertices, totalIndices));

            Span<byte> vertexSpan = new Span<byte>((byte*)meshBuildData.VertexData, meshBuildData.VertexCount * sizeof(Vertex));
            Span<byte> skinningSpan = new Span<byte>((byte*)meshBuildData.SkinningData, meshBuildData.VertexCount * sizeof(VertexSkinning));
            Span<byte> indexSpan = new Span<byte>((byte*)meshBuildData.IndexData, meshBuildData.IndexCount * sizeof(uint));
            

            int read;
            while ((read = reader.Read(vertexSpan)) != 0)
            {
                vertexSpan = vertexSpan.Slice(read);
            }

            if (hasSkeleton)
            {
                while ((read = reader.Read(skinningSpan)) != 0)
                {
                    skinningSpan = skinningSpan.Slice(read);
                }
            }
            
            while ((read = reader.Read(indexSpan)) != 0)
            {
                indexSpan = indexSpan.Slice(read);
            }
            
            game.ThreadSafety.EnsureMain(() => mesh.EndUpdate(meshBuildData));

            Material loadedMaterial = game.ResourceManager.Load<Material>(material);
            
            MeshData meshData = new MeshData(mesh, loadedMaterial, i);

            Meshes[i] = meshData;
        }

        Vector3 aabbMin = new Vector3();
        aabbMin.X = reader.ReadSingle();
        aabbMin.Y = reader.ReadSingle();
        aabbMin.Z = reader.ReadSingle();

        Vector3 aabbMax = new Vector3();
        aabbMax.X = reader.ReadSingle();
        aabbMax.Y = reader.ReadSingle();
        aabbMax.Z = reader.ReadSingle();

        BoundingBox = new AABB(aabbMin, aabbMax);

        int jointBoxes = reader.ReadInt32();
        for (int i = 0; i < jointBoxes; i++)
        {
            int key = reader.ReadInt32();
            
            aabbMin = new Vector3();
            aabbMin.X = reader.ReadSingle();
            aabbMin.Y = reader.ReadSingle();
            aabbMin.Z = reader.ReadSingle();

            aabbMax = new Vector3();
            aabbMax.X = reader.ReadSingle();
            aabbMax.Y = reader.ReadSingle();
            aabbMax.Z = reader.ReadSingle();

            MeshJointBoundingBoxes[key] = new AABB(aabbMin, aabbMax);
        }

        // Embedded animations
        int animationCount = reader.ReadInt32();
        for (int i = 0; i < animationCount; i++)
        {
            string key = reader.ReadString();
            AnimationClip clip = new AnimationClip(reader);
            Animations[key] = clip;
        }

        int assocCount = reader.ReadInt32();
        for (int i = 0; i < assocCount; i++)
        {
            string key = reader.ReadString();
            string value = reader.ReadString();
            AnimationClip clip = game.ResourceManager.Load<AnimationClip>(value);
            Animations[key] = clip;
        }
    }

    public AnimationClip? GetAnimation(string key, string? fallback = null)
    {
        if (Animations.TryGetValue(key, out var clip))
            return clip;
        if (fallback == null)
            return null;
        return game.ResourceManager.Load<AnimationClip>(fallback);
    }
    
    public AnimationClip? GetAnimationAssoc(string key, string? fallback = null)
    {
        if (Animations.TryGetValue(key, out var clip))
            return clip;
        if (fallback == null)
            return null;
        clip = game.ResourceManager.Load<AnimationClip>(fallback);
        if (clip != null!)
            Animations[key] = clip;
        return clip;
    }

    public DeformationCache GenerateDeformationCache()
    {
        DeformationCache deformationCache = new DeformationCache(Meshes.Length);

        for (int i = 0; i < Meshes.Length; i++)
        {
            deformationCache.AllocateBuffer(i, Meshes[i].ActualMesh.GetVertexBuffer()!.Length, BufferStorageMask.DynamicStorageBit);
        }
        
        return deformationCache;
    }
    
    public struct MeshData
    {
        public readonly Mesh ActualMesh;
        public readonly Material Material;
        public readonly int Index;

        public MeshData(Mesh actualMesh, Material material, int index)
        {
            ActualMesh = actualMesh;
            Material = material;
            Index = index;
        }
    }
    
    public class Loader : IResourceLoader<Model>
    {
        public Model LoadResource(ResourceData data)
        {
            Model model = new Model(data.ResourceManager.Game);
            model.Load(data.ResourceStream);
            return model;
        }
    }
}