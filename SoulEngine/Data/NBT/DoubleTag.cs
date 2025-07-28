namespace SoulEngine.Data.NBT;

public class DoubleTag : ValueTag<double>
{
    public double Data
    {
        get => Value;
        set => Value = value;
    }
    
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

    public override void Write(SNBTWriter writer)
    {
        writer.Append(Data + "D");
    }
}