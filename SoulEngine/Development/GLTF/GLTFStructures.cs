using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace SoulEngine.Development.GLTF;

public struct GLTFFile
{
    public GLTFFile()
    {
    }

    [JsonProperty(Required = Required.Always)]
    public Asset Asset;

    public string[] ExtensionsUsed = [];
    public string[] ExtensionsRequired = [];

    public Accessor[] Accessors = [];
    public Animation[] Animations = [];

    public Scene[] Scenes = [];
    public int? Scene;

    public Node[] Nodes = [];

    public Mesh[] Meshes = [];

    public Material[] Materials = [];

    public BufferView[] BufferViews = [];
    public Buffer[] Buffers = [];

    public Skin[] Skins = [];
}

public struct Asset
{
    public string Copyright;
    public string Generator;
    public string Version;
    public string MinVersion;
    public JObject Extensions;
    public JObject Extras;
}

public struct Accessor
{
    public Accessor()
    {
    }
    
    public int BufferView = -1;
    public int ByteOffset = 0;
    public AccessorComponentType ComponentType;
    public bool Normalized = false;
    public int Count;
    public string Type;
    public float[]? Max;
    public float[]? Min;
    
    // TODO: Sparse

    public string? Name;
    public JObject? Extensions;
    public JObject? Extras;
}

public enum AccessorComponentType
{
    Byte = 5120,
    UnsignedByte = 5121,
    Short = 5122,
    UnsignedShort = 5123,
    UnsignedInt = 5125,
    Float = 5126
}

public struct Scene
{
    public Scene()
    {
    }
    
    public int[] Nodes = [];
    public string? Name;
    public JObject? Extensions;
    public JObject? Extras;
}

public struct Node
{
    public Node()
    {
    }
    
    public int Camera = -1;
    public int[] Children = [];
    public int Skin = -1;
    public float[]? Matrix;
    public int Mesh = -1;
    public float[] Rotation = [0, 0, 0, 1];
    public float[] Scale = [1, 1, 1];
    public float[] Translation = [0, 0, 0];
    public int[] Weights = [];
    public string? Name;

    public JObject Extensions;
    public JObject Extras;
}

public struct Mesh
{
    public Primitive[] Primitives;
    public float[] Weights;
    public string? Name;

    public JObject Extensions;
    public JObject Extras;
    
    
    public struct Primitive
    {
        public Primitive()
        {
        }
        
        public Dictionary<string, int> Attributes;
        public int Indices = -1;
        public int? Material;
        public PrimitiveMode? Mode = PrimitiveMode.Triangles;

        public int[] Targets;

        public JObject Extensions;
        public JObject Extras;
    }
    
            
    public enum PrimitiveMode
    {
        Points = 0,
        Lines = 1,
        LineLoop = 2,
        LineStrip = 3,
        Triangles = 4,
        TriangleStrip = 5,
        TriangleFan = 6
    }
}


public struct Animation
{
    
}

public struct BufferView
{
    public BufferView() 
    {
        
    }
    
    public int Buffer;
    public int ByteOffset = 0;
    public int ByteLength;
    public int? ByteStride;
    public BufferViewTarget Target;
    public string? Name;

    public JObject? Extensions;
    public JObject? Extras;
}

public struct Buffer
{
    public string? Uri;
    public int ByteLength;
    public string Name;
    
    public JObject? Extensions;
    public JObject? Extras;
}

public enum BufferViewTarget
{
    ArrayBuffer = 34962,
    ElementArrayBuffer = 34963
}

public struct Material
{
    public string? Name;
}

public struct Skin
{
    public int InverseBindMatrices;
    public int? Skeleton;
    public int[] Joints;
    public string? Name;
    
    public JObject? Extensions;
    public JObject? Extras;
}