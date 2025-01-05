namespace SoulEngine.Data.NBT;

public class DoubleTag : Tag
{
    public double Data;
    
    public DoubleTag(string? name) : base(name, TagType.Double)
    {
    }
    
    public DoubleTag(string? name, double value) : base(name, TagType.Double)
    {
        Data = value;
    }

    public override void Read(BinaryReader reader)
    {
        Data = reader.ReadDouble();
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(Data);
    }
}