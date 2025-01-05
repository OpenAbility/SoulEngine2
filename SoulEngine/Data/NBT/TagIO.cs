using System.IO.Compression;
using System.Text;

namespace SoulEngine.Data.NBT;

public static class TagIO
{
    public static void WriteCompressed(Tag tag, Stream output, bool leaveOpen = true)
    {
        using GZipStream zipStream = new GZipStream(output, CompressionMode.Compress, leaveOpen);
        using BinaryWriter writer = new BinaryWriter(zipStream, Encoding.UTF8, false);
        
        Tag.WriteNamedTag(tag, writer);
    }
    
    public static void WriteUncompressed(Tag tag, Stream output, bool leaveOpen = true)
    {
        using BinaryWriter writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen);
        Tag.WriteNamedTag(tag, writer);
    }
    
    public static Tag ReadCompressed(Stream input, bool leaveOpen = true)
    {
        using GZipStream zipStream = new GZipStream(input, CompressionMode.Decompress, leaveOpen);
        using BinaryReader reader = new BinaryReader(zipStream, Encoding.UTF8, false);

        return Tag.ReadNamedTag(reader);
    }
    
    public static Tag ReadUncompressed(Stream input, bool leaveOpen = true)
    {
        using BinaryReader reader = new BinaryReader(input, Encoding.UTF8, leaveOpen);

        return Tag.ReadNamedTag(reader);
    }
}