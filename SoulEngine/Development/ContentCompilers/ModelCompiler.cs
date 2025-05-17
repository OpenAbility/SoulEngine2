using System.Text;
using Newtonsoft.Json;
using OpenTK.Mathematics;
using SoulEngine.Development.GLTF;
using SoulEngine.Rendering;
using SoulEngine.Util;

namespace SoulEngine.Development.ContentCompilers;

public class ModelCompiler : GLBContentCompiler
{
    
    public override void Recompile(ContentData contentData)
    {
        
        ModelDef modelDef = JsonConvert.DeserializeObject<ModelDef>(File.ReadAllText(contentData.InputFile.FullName));
        string glbPath = ResolvePath(contentData.InputFile.FullName, modelDef.Glb);

        GLTFLoader loader = new GLTFLoader(File.OpenRead(glbPath), false);

        using BinaryWriter writer = new BinaryWriter(File.OpenWrite(contentData.OutputFile.FullName), Encoding.UTF8, false);
        
        writer.Write('M');
        writer.Write('O');
        writer.Write('D');
        writer.Write('L');

        //modelDef.Skeleton ??= "skele/misc.skeleton";

        // There's a skeleton - write it
        if (modelDef.Skeleton != null && modelDef.Skeleton != "null")
        {
            writer.Write(true);
            writer.Write(modelDef.Skeleton);
            
            // There's no skinning to be done.
            if(!modelDef.Skin.HasValue || loader.File.Skins == null || loader.File.Skins.Length == 0)
                writer.Write(0);

            else
            {
                // Each mesh keeps track of the skin indices of each bone
                // These can then be quickly mapped by the engine into an int-int mapping
                // (or an array if you will).
            
                // We need to do it like this because the model compiler knows nothing of
                // the requested skeleton layout
                Skin skin = loader.File.Skins[modelDef.Skin!.Value];
                writer.Write(skin.Joints.Length);

                for (int i = 0; i < skin.Joints.Length; i++)
                {
                    writer.Write(loader.File.Nodes[skin.Joints[i]].Name!);
                    writer.Write(i);
                }
            
            }
        }
        else
        {
            writer.Write(false);
        }
        
        
        
        int totalMeshCount = 0;

        for (int i = 0; i < loader.File.Meshes.Length; i++)
        {
            for (int j = 0; j < loader.File.Meshes[i].Primitives.Length; j++)
            {
                // TODO: CHECK TRIS. Newtonsoft.Json fucks up some stuff here.
                if (loader.File.Meshes[i].Primitives[j].Indices != -1)
                    totalMeshCount++;
            }
        }
        
        writer.Write(totalMeshCount);

        for (int meshIndex = 0; meshIndex < loader.File.Meshes.Length; meshIndex++)
        {
            Mesh mesh = loader.File.Meshes[meshIndex];
            for (int primitiveIndex = 0; primitiveIndex < mesh.Primitives.Length; primitiveIndex++)
            {
                Mesh.Primitive primitive = mesh.Primitives[primitiveIndex];
                
                //if(primitive.Mode != Mesh.PrimitiveMode.Triangles)
                //    continue;
                if(primitive.Indices == -1)
                    continue;
                
                // Ugly chaining. I genuinely don't know.
                if(primitive.Material.HasValue && 
                   loader.File.Materials[primitive.Material.Value].Name != null && 
                   modelDef.Materials.TryGetValue(loader.File.Materials[primitive.Material.Value].Name!, out string? materialPath))
                    writer.Write(materialPath);
                else
                    writer.Write("default.mat");
                
                Accessor? vertexAccessor = loader.GetMeshAttribute(meshIndex, primitiveIndex, "POSITION");
                if (vertexAccessor == null)
                    throw new Exception("Model primitive does not provide vertex data!");
                if (vertexAccessor.Value.Type != "VEC3")
                    throw new Exception("Model primitive vertex data is of unsupported type " +
                                        vertexAccessor.Value.Type);
                if (vertexAccessor.Value.ComponentType != AccessorComponentType.Float)
                    throw new Exception("Model primitive vertex data is of unsupported type " +
                                        vertexAccessor.Value.ComponentType);
                
                Accessor? normalAccessor = loader.GetMeshAttribute(meshIndex, primitiveIndex, "NORMAL");
                Accessor? uvAccessor = loader.GetMeshAttribute(meshIndex, primitiveIndex, "TEXCOORD_0");
                Accessor? uv2Accessor = loader.GetMeshAttribute(meshIndex, primitiveIndex, "TEXCOORD_1");
                Accessor? colorAccessor = loader.GetMeshAttribute(meshIndex, primitiveIndex, "COLOR_0");
                
                Accessor? jointsAccessor = loader.GetMeshAttribute(meshIndex, primitiveIndex, "JOINTS_0");
                Accessor? weightsAccessor = loader.GetMeshAttribute(meshIndex, primitiveIndex, "WEIGHTS_0");
                
                Accessor indexAccessor = loader.File.Accessors[primitive.Indices];

                int totalVertices = vertexAccessor.Value.Count;
                writer.Write(totalVertices);
                
                int totalIndices = indexAccessor.Count;
                writer.Write(totalIndices);

                for (int i = 0; i < totalVertices; i++)
                {
                    
                    Vertex vertex = new Vertex();
                    vertex.Colour = Colour.White;
                    
                    // TODO: Account for different input types
                    
                    vertex.Position = new Vector3(
                        loader.GetAccessor(vertexAccessor.Value, i * 3 + 0).CastStruct<float, byte>(),
                        loader.GetAccessor(vertexAccessor.Value, i * 3 + 1).CastStruct<float, byte>(),
                        loader.GetAccessor(vertexAccessor.Value, i * 3 + 2).CastStruct<float, byte>()
                    );

                    if (normalAccessor.HasValue)
                    {
                        vertex.Normal = new Vector3(
                            loader.GetAccessor(normalAccessor.Value, i * 3 + 0).CastStruct<float, byte>(),
                            loader.GetAccessor(normalAccessor.Value, i * 3 + 1).CastStruct<float, byte>(),
                            loader.GetAccessor(normalAccessor.Value, i * 3 + 2).CastStruct<float, byte>()
                        );
                    }
                    
                    if (uvAccessor.HasValue)
                    {
                        // Flip Y uv 'cause glTF is Y-down. We are Y-up :D
                        vertex.UV = new Vector2(
                            loader.GetAccessor(uvAccessor.Value, i * 2 + 0).CastStruct<float, byte>(),
                            1 - loader.GetAccessor(uvAccessor.Value, i * 2 + 1).CastStruct<float, byte>()
                        );
                    }
                    
                    if (uv2Accessor.HasValue)
                    {
                        // Flip Y uv 'cause glTF is Y-down. We are Y-up :D
                        vertex.UV2 = new Vector2(
                            loader.GetAccessor(uv2Accessor.Value, i * 2 + 0).CastStruct<float, byte>(),
                            1 - loader.GetAccessor(uv2Accessor.Value, i * 2 + 1).CastStruct<float, byte>()
                        );
                    }
                    
                    if (jointsAccessor.HasValue)
                    {
                        vertex.Indices = new JointIndices(
                            loader.GetAccessor(jointsAccessor.Value, i * 4 + 0).CastStruct<byte, byte>(),
                            loader.GetAccessor(jointsAccessor.Value, i * 4 + 1).CastStruct<byte, byte>(),
                            loader.GetAccessor(jointsAccessor.Value, i * 4 + 2).CastStruct<byte, byte>(),
                            loader.GetAccessor(jointsAccessor.Value, i * 4 + 3).CastStruct<byte, byte>()
                        );
                    }
                    
                    if (weightsAccessor.HasValue)
                    {
                        vertex.Weights = new Vector4(
                            loader.GetAccessor(weightsAccessor.Value, i * 4 + 0).CastStruct<float, byte>(),
                            loader.GetAccessor(weightsAccessor.Value, i * 4 + 1).CastStruct<float, byte>(),
                            loader.GetAccessor(weightsAccessor.Value, i * 4 + 2).CastStruct<float, byte>(),
                            loader.GetAccessor(weightsAccessor.Value, i * 4 + 3).CastStruct<float, byte>()
                        );
                    }
                    
                    // TODO: COLOURS
                    
                    
                    writer.WriteStruct(vertex);
                }

                for (int i = 0; i < totalIndices; i++)
                {
                    uint index = 0;
                    
                    if (indexAccessor.ComponentType == AccessorComponentType.Byte ||
                        indexAccessor.ComponentType == AccessorComponentType.UnsignedByte)
                    {
                        index = loader.GetAccessor(indexAccessor, i)[0];
                    } else if (indexAccessor.ComponentType == AccessorComponentType.Short ||
                               indexAccessor.ComponentType == AccessorComponentType.UnsignedShort)
                    {
                        index = loader.GetAccessor(indexAccessor, i).CastStruct<ushort, byte>();
                    }
                    else if (indexAccessor.ComponentType == AccessorComponentType.UnsignedInt)
                    {
                        index = loader.GetAccessor(indexAccessor, i).CastStruct<uint, byte>();
                    }
                    else
                    {
                        throw new Exception("Weird index format " + indexAccessor.ComponentType);
                    }
                    
                    writer.Write(index);
                }

            }
        }

    }

    public override string GetCompiledPath(string path)
    {
        return Path.ChangeExtension(path, "mdl");
    }
    
    private struct ModelDef()
    {
        public string Glb = "NULL";
        public Dictionary<string, string> Materials = new();
        public int? Skin;
        public string Skeleton = "skele/misc.skeleton";
    }
}