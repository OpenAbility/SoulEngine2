using System.Text;
using OpenAbility.Logging;
using SoulEngine.Content;
using SoulEngine.Core;
using SoulEngine.Rendering;
using SoulEngine.Resources;
using SoulEngine.Util;
using Material = SoulEngine.Resources.Material;

namespace SoulEngine.Models;

[Resource(typeof(Loader))]
[ExpectedExtensions(".mdl")]
public class Model : Resource
{

    public MeshData[] Meshes { get; private set; } = [];
    public Skeleton? Skeleton { get; private set; }

    public readonly Dictionary<int, int> skeletonToMeshJoints = new Dictionary<int, int>();

    private readonly Game game;
    public Model(Game game)
    {
        this.game = game;
    }
    
    private unsafe void Load(ResourceManager resourceManager, string id, ContentContext content)
    {
        BinaryReader reader = new BinaryReader(content.Load(id)!);

        if (Encoding.UTF8.GetString(reader.ReadBytes(4)) != "MODL")
            throw new Exception("File is not SoulEngine mdl!");

        bool hasSkeleton = reader.ReadBoolean();
        if (hasSkeleton)
        {
            string skeletonPath = reader.ReadString();
            Skeleton = resourceManager.Load<Skeleton>(skeletonPath);
            
            int skinJointCount = reader.ReadInt32();
        
            for (int i = 0; i < skinJointCount; i++)
            {
                string name = reader.ReadString();
                int index = reader.ReadInt32();

                SkeletonJointData? data = Skeleton.GetJoint(name);
                if(data != null)
                    skeletonToMeshJoints[data.SkeletonID] = index;
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
            /*
            for (int j = 0; j < totalVertices; j++)
            {
                meshBuildData.Vertices[j] = reader.ReadStruct<Vertex>();
            }


            for (int j = 0; j < totalIndices; j++)
            {
                meshBuildData.Indices[j] = reader.ReadUInt32();
            }
            */
            
            game.ThreadSafety.EnsureMain(() => mesh.EndUpdate(meshBuildData));

            Material loadedMaterial = resourceManager.Load<Material>(material);
            
            MeshData meshData = new MeshData(mesh, loadedMaterial);

            Meshes[i] = meshData;
        }
        
    }
    
    public struct MeshData
    {
        public readonly Mesh ActualMesh;
        public readonly Material Material;

        public MeshData(Mesh actualMesh, Material material)
        {
            ActualMesh = actualMesh;
            Material = material;
        }
    }
    
    public class Loader : IResourceLoader<Model>
    {
        public Model LoadResource(ResourceManager resourceManager, string id, ContentContext content)
        {
            Model model = new Model(resourceManager.Game);
            model.Load(resourceManager, id, content);
            return model;
        }
    }
}