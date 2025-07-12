using System.Text;
using Newtonsoft.Json;
using OpenTK.Mathematics;
using SoulEngine.Development.GLTF;
using SoulEngine.Mathematics;
using SoulEngine.Rendering;
using SoulEngine.Util;
using Mesh = SoulEngine.Development.GLTF.Mesh;

namespace SoulEngine.Development.ContentCompilers;

public class ModelCompiler : GLBContentCompiler
{

    public override void Recompile(ContentData contentData)
    {

        ModelDef modelDef = JsonConvert.DeserializeObject<ModelDef>(File.ReadAllText(contentData.InputFile.FullName));

        modelDef.MaterialPrefix ??= "mat/";
        modelDef.Materials ??= new Dictionary<string, string>();
        
        string glbPath = ResolvePath(contentData.InputFile.FullName, modelDef.Glb);

        GLTFLoader loader = new GLTFLoader(File.OpenRead(glbPath), false);

        using BinaryWriter writer =
            new BinaryWriter(File.OpenWrite(contentData.OutputFile.FullName), Encoding.UTF8, false);

        Dictionary<int, AABB> jointBoundingBoxes = new Dictionary<int, AABB>();
        AABB modelBoundingBox = AABB.InvertedInfinity;

        writer.Write('M');
        writer.Write('O');
        writer.Write('D');
        writer.Write('L');

        //modelDef.Skeleton ??= "skele/misc.skeleton";

        bool hasSkeleton = false;

        // There's a skeleton - write it
        if (modelDef.Skeleton != null && modelDef.Skeleton != "null")
        {
            hasSkeleton = true;
            writer.Write(true);
            writer.Write(modelDef.Skeleton);

            // There's no skinning to be done.
            if (!modelDef.Skin.HasValue || loader.File.Skins == null || loader.File.Skins.Length == 0)
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
                    
                    jointBoundingBoxes[i] = AABB.InvertedInfinity;
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
                if (primitive.Indices == -1)
                    continue;

                // Ugly chaining. I genuinely don't know.

                string materialName = "default";
                if (primitive.Material.HasValue)
                {
                    materialName = loader.File.Materials[primitive.Material.Value].Name ?? "default";
                }
                
                if (modelDef.Materials.TryGetValue(materialName, out string? materialPath))
                    writer.Write(materialPath);
                else
                    writer.Write(modelDef.MaterialPrefix + materialName + ".mat");

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

                    modelBoundingBox.PushPoint(vertex.Position);

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

                    /*
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
                    */

                    // TODO: COLOURS


                    writer.WriteStruct(vertex);
                }

                if (hasSkeleton)
                {
                    for (int i = 0; i < totalVertices; i++)
                    {
                        VertexSkinning vertex = new VertexSkinning();
                        
                        

                        if (jointsAccessor.HasValue)
                        {
                            Vector3 vertexPosition = new Vector3(
                                loader.GetAccessor(vertexAccessor.Value, i * 3 + 0).CastStruct<float, byte>(),
                                loader.GetAccessor(vertexAccessor.Value, i * 3 + 1).CastStruct<float, byte>(),
                                loader.GetAccessor(vertexAccessor.Value, i * 3 + 2).CastStruct<float, byte>()
                            );
                            
                            // TODO: More than 255 joints?
                            vertex.Indices = new JointIndices(
                                loader.GetAccessor(jointsAccessor.Value, i * 4 + 0).CastStruct<byte, byte>(),
                                loader.GetAccessor(jointsAccessor.Value, i * 4 + 1).CastStruct<byte, byte>(),
                                loader.GetAccessor(jointsAccessor.Value, i * 4 + 2).CastStruct<byte, byte>(),
                                loader.GetAccessor(jointsAccessor.Value, i * 4 + 3).CastStruct<byte, byte>()
                            );

                            for (int j = 0; j < 4; j++)
                            {
                                uint index = vertex.Indices[j];

                                jointBoundingBoxes[(int)index] =
                                    jointBoundingBoxes[(int)index].PushPoint(vertexPosition);
                            }
                        }

                        if (weightsAccessor.HasValue)
                        {
                            vertex.Weights = new Vector4(
                                loader.GetAccessor(weightsAccessor!.Value, i * 4 + 0).CastStruct<float, byte>(),
                                loader.GetAccessor(weightsAccessor.Value, i * 4 + 1).CastStruct<float, byte>(),
                                loader.GetAccessor(weightsAccessor.Value, i * 4 + 2).CastStruct<float, byte>(),
                                loader.GetAccessor(weightsAccessor.Value, i * 4 + 3).CastStruct<float, byte>()
                            );
                        }

                        writer.WriteStruct(vertex);
                    }
                }


                for (int i = 0; i < totalIndices; i++)
                {
                    uint index = 0;

                    if (indexAccessor.ComponentType == AccessorComponentType.Byte ||
                        indexAccessor.ComponentType == AccessorComponentType.UnsignedByte)
                    {
                        index = loader.GetAccessor(indexAccessor, i)[0];
                    }
                    else if (indexAccessor.ComponentType == AccessorComponentType.Short ||
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

        if (modelBoundingBox.Invalid)
            modelBoundingBox = new AABB();
        
        writer.Write(modelBoundingBox.Min.X);
        writer.Write(modelBoundingBox.Min.Y);
        writer.Write(modelBoundingBox.Min.Z);
        
        writer.Write(modelBoundingBox.Max.X);
        writer.Write(modelBoundingBox.Max.Y);
        writer.Write(modelBoundingBox.Max.Z);

        if (hasSkeleton)
        {
            writer.Write(jointBoundingBoxes.Count);
            foreach (var joint in jointBoundingBoxes)
            {
                writer.Write(joint.Key);

                AABB aabb = joint.Value;
                if (aabb.Invalid) aabb = new AABB();
                
                writer.Write(aabb.Min.X);
                writer.Write(aabb.Min.Y);
                writer.Write(aabb.Min.Z);
                writer.Write(aabb.Max.X);
                writer.Write(aabb.Max.Y);
                writer.Write(aabb.Max.Z);
            }
        }

        modelDef.Animations ??= [];
        
        writer.Write(modelDef.Animations.Length);
        for (int i = 0; i < modelDef.Animations.Length; i++)
        {
            GLTF.Animation? animation = Array.Find(loader.File.Animations, anim => anim.Name == modelDef.Animations[i]);
            if (animation == null)
                throw new Exception("Could not find animation " + modelDef.Animations[i]);

            writer.Write(animation.Value.Name!);
            AnimationCompiler.WriteAnimations(writer, false, animation.Value, loader);
        }
        
        modelDef.AnimationsAssoc ??= new Dictionary<string, string>();
        
        writer.Write(modelDef.AnimationsAssoc.Count);
        foreach (var assoc in modelDef.AnimationsAssoc)
        {
            writer.Write(assoc.Key);
            writer.Write(assoc.Value);
        }

    }

    public override string GetCompiledPath(string path)
    {
        return Path.ChangeExtension(path, "mdl");
    }

    private struct ModelDef()
    {
        [JsonProperty("glb", Required = Required.Always)] public string Glb = "NULL";
        [JsonProperty("materials", Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)] public Dictionary<string, string> Materials = new();
        [JsonProperty("skin", Required = Required.Default)] public int? Skin;
        [JsonProperty("skeleton", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)] public string Skeleton = "skele/misc.skeleton";
        [JsonProperty("mat_pfx", Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)] public string? MaterialPrefix = "mat/";
        [JsonProperty("animations", Required = Required.DisallowNull,
            DefaultValueHandling = DefaultValueHandling.Populate)]
        public string[] Animations = [];
        
        [JsonProperty("animations_assoc", Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)] public Dictionary<string, string> AnimationsAssoc = new();
    }
}