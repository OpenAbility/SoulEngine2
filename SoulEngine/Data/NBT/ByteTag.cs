namespace SoulEngine.Data.NBT;

public class ByteTag : ValueTag<byte>
{
    public byte Data
    {
        get => Value;
        set => Value = value;
    }
    
    public ByteTag(string? name) : base(name, TagType.Byte)
    {
    }
    
    public ByteTag(string? name, byte value) : base(name, TagType.Byte)
    {
        Data = value;
    }

    public override void Read(BinaryReader reader)
    {
        Data = reader.ReadByte();
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(Data);
    }

    public override void Write(SNBTWriter writer)
    {
        writer.Append(Data + "B");
    }
}