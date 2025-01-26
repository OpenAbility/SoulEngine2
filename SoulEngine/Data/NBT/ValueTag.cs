namespace SoulEngine.Data.NBT;

public abstract class ValueTag<T> : Tag
{
    public T Value;

    protected ValueTag(string? name, TagType type) : base(name, type)
    {
    }

    public override string ToString()
    {
        return GetType().Name + ": " + Value;
    }
}