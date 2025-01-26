namespace SoulEngine.Data.NBT;

public class FloatTag : ValueTag<float>
{
    public float Data
    {
        get => Value;
        set => Value = value;
    }
    
    public FloatTag(string? name) : base(name, TagType.Float)
    {
    }
    
    public FloatTag(string? name, float value) : base(name, TagType.Float)
    {
        Data = value;
    }

    public override void Read(BinaryReader reader)
    {
        Data = reader.ReadSingle();
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(Data);
    }

    public override void Write(SNBTWriter writer)
    {
        writer.Append(Data + "f");
    }
}