using System.IO.Compression;
using System.Text;
using Be.IO;

namespace SoulEngine.Data.NBT;

public static class TagIO
{
    public static void WriteCompressed(Tag tag, Stream output, bool leaveOpen = true)
    {
        using GZipStream zipStream = new GZipStream(output, CompressionMode.Compress, leaveOpen);
        using BeBinaryWriter writer = new BeBinaryWriter(zipStream, Encoding.UTF8, false);
        
        Tag.WriteNamedTag(tag, writer);
    }
    
    public static void WriteUncompressed(Tag tag, Stream output, bool leaveOpen = true)
    {
        using BeBinaryWriter writer = new BeBinaryWriter(output, Encoding.UTF8, leaveOpen);
        Tag.WriteNamedTag(tag, writer);
    }
    
    public static Tag ReadCompressed(Stream input, bool leaveOpen = true)
    {
        using GZipStream zipStream = new GZipStream(input, CompressionMode.Decompress, leaveOpen);
        using BeBinaryReader reader = new BeBinaryReader(zipStream, Encoding.UTF8, false);

        return Tag.ReadNamedTag(reader);
    }
    
    public static Tag ReadUncompressed(Stream input, bool leaveOpen = true)
    {
        using BeBinaryReader reader = new BeBinaryReader(input, Encoding.UTF8, leaveOpen);

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