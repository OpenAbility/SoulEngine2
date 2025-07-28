namespace SoulEngine.Data.NBT;

public class ShortTag : ValueTag<short>
{
    public short Data
    {
        get => Value;
        set => Value = value;
    }
    
    public ShortTag(string? name) : base(name, TagType.Short)
    {
    }
    
    public ShortTag(string? name, short value) : base(name, TagType.Short)
    {
        Data = value;
    }

    public override void Read(BinaryReader reader)
    {
        Data = reader.ReadInt16();
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(Data);
    }

    public override void Write(SNBTWriter writer)
    {
        writer.Append(Data + "S");
    }
}