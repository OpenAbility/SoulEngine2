namespace SoulEngine.Data.NBT;

public class StringTag : Tag
{
    public string Data;
    
    public StringTag(string? name) : base(name, TagType.String)
    {
    }
    
    public StringTag(string? name, string value) : base(name, TagType.String)
    {
        Data = value;
    }

    public override void Read(BinaryReader reader)
    {
        short length = reader.ReadInt16();
        Data = new string(reader.ReadChars(length));
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write((short)Data.Length);
        writer.Write(Data.ToCharArray());
    }
}