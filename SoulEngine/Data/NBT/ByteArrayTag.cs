namespace SoulEngine.Data.NBT;

public class ByteArrayTag : ArrayTag<byte>
{
    public ByteArrayTag(string? name) : base(name, TagType.ByteArray)
    {
    }
    
    public ByteArrayTag(string? name, byte[] value) : base(name, TagType.ByteArray)
    {
        Value = value;
    }

    public override void Read(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        Value = reader.ReadBytes(length);
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(Value.Length);
        writer.Write(Value);
    }
}