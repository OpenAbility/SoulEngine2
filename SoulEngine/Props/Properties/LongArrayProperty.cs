using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class LongArrayProperty : PropProperty<long[]>
{
    public LongArrayProperty(string name, long[] defaultValue) : base(name, defaultValue)
    {
    }

    public override void Edit()
    {
        
    }

    public override Tag Save()
    {
        return new LongArrayTag(Name, Value);
    }

    public override void Load(Tag tag)
    {
        Value = ((LongArrayTag)tag).Value;
    }
}