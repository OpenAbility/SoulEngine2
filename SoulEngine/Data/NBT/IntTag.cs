namespace SoulEngine.Data.NBT;

public class IntTag : Tag
{
    public int Data;
    
    public IntTag(string? name) : base(name, TagType.Int)
    {
    }
    
    public IntTag(string? name, int value) : base(name, TagType.Int)
    {
        Data = value;
    }

    public override void Read(BinaryReader reader)
    {
        Data = reader.ReadInt32();
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(Data);
    }

    public override void Write(SNBTWriter writer)
    {
        writer.Append(Data.ToString());
    }
}