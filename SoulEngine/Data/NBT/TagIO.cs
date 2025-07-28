using System.IO.Compression;
using System.Text;
using SoulEngine.Util;

namespace SoulEngine.Data.NBT;

public static class TagIO
{
    public static void WriteCompressed(Tag tag, Stream output, bool leaveOpen = true)
    {
        using GZipStream zipStream = new GZipStream(output, CompressionMode.Compress, leaveOpen);
        using NetworkBinaryWriter writer = new NetworkBinaryWriter(zipStream, Encoding.UTF8, false);
        
        Tag.WriteNamedTag(tag, writer);
    }
    
    public static void WriteUncompressed(Tag tag, Stream output, bool leaveOpen = true)
    {
        using NetworkBinaryWriter writer = new NetworkBinaryWriter(output, Encoding.UTF8, leaveOpen);
        Tag.WriteNamedTag(tag, writer);
    }
    
    public static Tag ReadCompressed(Stream input, bool leaveOpen = true)
    {
        using GZipStream zipStream = new GZipStream(input, CompressionMode.Decompress, leaveOpen);
        using NetworkBinaryReader reader = new NetworkBinaryReader(zipStream, Encoding.UTF8, false);

        return Tag.ReadNamedTag(reader);
    }
    
    public static Tag ReadUncompressed(Stream input, bool leaveOpen = true)
    {
        using NetworkBinaryReader reader = new NetworkBinaryReader(input, Encoding.UTF8, leaveOpen);

        return Tag.ReadNamedTag(reader);
    }

    public static string WriteSNBT(Tag tag)
    {
        StringWriter writer = new StringWriter();
        using SNBTWriter snbtWriter = new SNBTWriter(writer, true);
        
        tag.WriteNamed(snbtWriter);
        
        writer.Flush();
        writer.Close();

        return writer.ToString();
    }
    
    public static Tag ReadSNBT(string snbt)
    {
        return TagParser.Parse(snbt);
    }
}