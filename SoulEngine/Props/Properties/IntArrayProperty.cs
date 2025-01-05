using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class IntArrayProperty : PropProperty<int[]>
{
    public IntArrayProperty(string name, int[] defaultValue) : base(name, defaultValue)
    {
    }

    public override void Edit()
    {
        
    }

    public override Tag Save()
    {
        return new IntArrayTag(Name, Value);
    }

    public override void Load(Tag tag)
    {
        Value = ((IntArrayTag)tag).Value;
    }
}