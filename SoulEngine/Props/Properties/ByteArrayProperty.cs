using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class ByteArrayProperty : SerializedProperty<byte[]>
{
    public ByteArrayProperty(string name, byte[] defaultValue) : base(name, defaultValue)
    {
    }

    public override void Edit()
    {
        
    }

    public override Tag Save()
    {
        return new ByteArrayTag(Name, Value);
    }

    public override void Load(Tag tag)
    {
        Value = ((ByteArrayTag)tag).Value;
    }
}