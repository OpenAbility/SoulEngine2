namespace SoulEngine.Data.NBT;

public class LongTag : ValueTag<long>
{
    public long Data
    {
        get => Value;
        set => Value = value;
    }
    
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

    public override void Write(SNBTWriter writer)
    {
        writer.Append(Data + "L");
    }
}