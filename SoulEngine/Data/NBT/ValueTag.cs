namespace SoulEngine.Data.NBT;

public abstract class ValueTag<T>(string? name, TagType type) : Tag(name, type)
{
    public T Value = default!;

    public override string ToString()
    {
        return GetType().Name + ": " + Value;
    }
}