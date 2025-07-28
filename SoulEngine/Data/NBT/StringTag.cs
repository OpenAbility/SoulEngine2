using System.Text;
using SoulEngine.Util;

namespace SoulEngine.Data.NBT;

public class StringTag : ValueTag<string>
{
    public string Data
    {
        get => Value;
        set => Value = value;
    }
    
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
        Data = Encoding.UTF8.GetString(reader.ReadBytes(length));
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write((short)Data.Length);
        writer.Write(Data.ToCharArray());
    }

    public override void Write(SNBTWriter writer)
    {
        writer.Append('"' + EngineUtility.JsonEscape(Data) + '"');
    }
}