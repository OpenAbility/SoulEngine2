using Newtonsoft.Json;
using OpenTK.Mathematics;
using SoulEngine.Data.NBT;
using SoulEngine.Development.GLTF;
using SoulEngine.Util;

namespace SoulEngine.Development.ContentCompilers;

public class SkeletonCompiler : GLBContentCompiler
{
    private static readonly Version Version = new Version(0, 1, 0);
    
    public override void Recompile(ContentData contentData)
    {
        SkeleDef skeleDef = JsonConvert.DeserializeObject<SkeleDef>(File.ReadAllText(contentData.InputFile.FullName));
        string glbPath = ResolvePath(contentData.InputFile.FullName, skeleDef.Glb);

        GLTFLoader loader = new GLTFLoader(File.OpenRead(glbPath), false);

        CompoundTag tag = new CompoundTag(Path.GetRelativePath(contentData.OutputDirectory.FullName, contentData.OutputFile.FullName));
        
        tag.SetString("compiler", "SoulEngine " + EngineData.EngineVersion + " w/ CC " + EngineData.ContentCompiler + " - SkeletonCompiler v. " + Version);
        
        tag.SetString("engine", EngineData.EngineName);

        {
            CompoundTag versionTag = new CompoundTag("engineVersion");
            versionTag.SetInt("major", EngineData.EngineVersion.Major);
            versionTag.SetInt("minor", EngineData.EngineVersion.Minor);
            versionTag.SetInt("revision", EngineData.EngineVersion.Revision);
            
            tag.SetCompound("engineVersion", versionTag);
        }

        Skin skin = loader.File.Skins[skeleDef.Skeleton];
        
        tag.SetString("glbSkinName", skin.Name ?? "No Name");
        

        Accessor inverseAccessor = loader.File.Accessors[skin.InverseBindMatrices];
        
        int skeleton = FindSkeleton(loader, skeleDef.RootNode);
        
        tag.Add("joints", BakeNode(skeleton, inverseAccessor, loader, skin));
        
        
        TagIO.WriteCompressed(tag, contentData.OutputFile.OpenWrite(), false);
    }

    private int FindSkeleton(GLTFLoader loader, string name)
    {
        for (int i = 0; i < loader.File.Nodes.Length; i++)
        {
            if (loader.File.Nodes[i].Name == name)
                return i;
        }

        throw new Exception("Could not find root node '" + name + "'!");
    }

    private CompoundTag BakeNode(int nodeIndex, Accessor accessor, GLTFLoader loader, Skin skin)
    {
        Node node = loader.File.Nodes[nodeIndex];

        CompoundTag tag = new CompoundTag(node.Name);
        
        tag.SetString("name", node.Name!);

        ListTag childTag = new ListTag("children");

        if (node.Children != null)
        {
            for (int i = 0; i < node.Children.Length; i++)
            {
                childTag.Add(BakeNode(node.Children[i], accessor, loader, skin));
            }
        }

        Matrix4 nodeMatrix = new Matrix4();
        
        if (node.Matrix == null)
        {
            node.Scale ??= [1, 1, 1];
            node.Rotation ??= [0, 0, 0, 1];
            node.Translation ??= [0, 0, 0];
            
            nodeMatrix = Matrix4.CreateScale(node.Scale[0], node.Scale[1], node.Scale[2]) *
                         Matrix4.CreateFromQuaternion(new Quaternion(node.Rotation[0], node.Rotation[1],
                             node.Rotation[2], node.Rotation[3]))
                         * Matrix4.CreateTranslation(node.Translation[0], node.Translation[1], node.Translation[2]);
        }
        else
        {
            nodeMatrix = EngineUtility.ArrayToMatrix(node.Matrix);
        }

        ListTag matrixTag = new ListTag("matrix");
        float[] m = new float[16];
        
        nodeMatrix.MatrixToArray(ref m);
        for (int i = 0; i < 16; i++)
        {
            matrixTag.Add(new FloatTag(null, m[i]));
        }

        tag["matrix"] = matrixTag;

        int jointIndex = Array.IndexOf(skin.Joints, nodeIndex);
        
        tag.SetInt("joint", jointIndex);
        
        if (jointIndex != -1)
        {

            ListTag inverseTag = new ListTag("inverseMatrix");
            Matrix4 matrix = EngineUtility.ArrayToMatrix(loader.GetAccessor(accessor, jointIndex * 16, 16).CastStructs<float, byte>());
            
            matrix.MatrixToArray(ref m);
            for (int i = 0; i < 16; i++)
            {
                inverseTag.Add(new FloatTag(null, m[i]));
            }

            tag["inverseMatrix"] = inverseTag;
        }

        tag["children"] = childTag;

        return tag;
    }

    public override string GetCompiledPath(string path)
    {
        return Path.ChangeExtension(path, "skeleton");
    }

    private struct SkeleDef()
    {
        public string Glb = "NULL";
        public int Skeleton = 0;
        public string RootNode = "NULL";
    }
}