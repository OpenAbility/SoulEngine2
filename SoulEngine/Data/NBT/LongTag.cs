namespace SoulEngine.Data.NBT;

public class LongTag : Tag
{
    public long Data;
    
    public LongTag(string? name) : base(name, TagType.Long)
    {
    }
    
    public LongTag(string? name, long value) : base(name, TagType.Long)
    {
        Data = value;
    }

    public override void Read(BinaryReader reader)
    {
        Data = reader.ReadInt64();
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(Data);
    }
}