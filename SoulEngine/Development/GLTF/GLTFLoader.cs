using System.Drawing;
using System.Text;
using Newtonsoft.Json;

namespace SoulEngine.Development.GLTF;

/// <summary>
/// Loads a full GLB file. Not GLTF, GLB.
/// </summary>
public class GLTFLoader
{

    private readonly byte[] binaryData;

    public readonly GLTFFile File;

    public GLTFLoader(Stream stream, bool leaveOpen = true)
    {
        using BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen);

        if (reader.ReadUInt32() != 0x46546C67)
            throw new Exception("Invalid GLB header!");

        if (reader.ReadUInt32() != 2)
            throw new Exception("GLB is not of version 2!");

        uint length = reader.ReadUInt32();
        
        // First chunk MUST be the JSON chunk, so we just read it.

        (byte[] jsonBinary, uint jsonType) = ReadChunk(reader);

        if (jsonType != 0x4E4F534A)
            throw new Exception("Invalid JSON chunk!");

        string json = Encoding.UTF8.GetString(jsonBinary);

        (binaryData, uint binaryType) = ReadChunk(reader);

        if (binaryType != 0x004E4942)
            throw new Exception("Invalid BIN chunk!");

        File = JsonConvert.DeserializeObject<GLTFFile>(json);
    }

    private (byte[] data, uint type) ReadChunk(BinaryReader reader)
    {
        uint length = reader.ReadUInt32();
        uint type = reader.ReadUInt32();

        return (reader.ReadBytes((int)length), type);
    }

    public Span<byte> ViewBuffer(BufferView view)
    {
        return GetBuffer(view.Buffer).Slice(view.ByteOffset, view.ByteLength);
    }

    public Mesh GetMesh(int index)
    {
        return File.Meshes[index];
    }
    
    public Mesh.Primitive GetMeshPrimitive(int mesh, int primitive)
    {
        return File.Meshes[mesh].Primitives[primitive];
    }
    
    public Accessor? GetMeshAttribute(int mesh, int primitive, string attribute)
    {
        var prim = GetMeshPrimitive(mesh, primitive);
        if (prim.Attributes.TryGetValue(attribute, out var idx))
            return File.Accessors[idx];
        return null;
    }

    public Span<byte> GetAccessor(Accessor accessor, int index, int count = 1)
    {
        BufferView bufferView = File.BufferViews[accessor.BufferView];

        Span<byte> view = ViewBuffer(bufferView);

        int accessorSize = GetSize(accessor.ComponentType);

        if (bufferView.ByteStride == null)
            return view.Slice(accessor.ByteOffset + accessorSize * index, accessorSize * count);
        return view.Slice(accessor.ByteOffset + bufferView.ByteStride.Value * index, accessorSize * count);
    }

    public static int GetSize(AccessorComponentType componentType)
    {
        return componentType switch
        {
            AccessorComponentType.Byte => sizeof(sbyte),
            AccessorComponentType.UnsignedByte => sizeof(byte),
            AccessorComponentType.Short => sizeof(short),
            AccessorComponentType.UnsignedShort => sizeof(ushort),
            AccessorComponentType.UnsignedInt => sizeof(uint),
            AccessorComponentType.Float => sizeof(float),
            _ => throw new ArgumentOutOfRangeException(nameof(componentType), componentType, null)
        };
    }

    public Span<byte> GetBuffer(int buffer)
    {
        if (buffer != 0)
            throw new Exception("Only buffer 0 (embedded) is supported right now!");

        return binaryData;
    }
    
}