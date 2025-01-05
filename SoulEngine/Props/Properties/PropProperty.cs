using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public abstract class PropProperty
{
    public readonly string Name;

    public PropProperty(string name)
    {
        Name = name;
    }
    
    #if DEVELOPMENT
    public abstract void Edit();
    
    #endif
    
    public abstract Tag Save();
    public abstract void Load(Tag tag);
}

public abstract class PropProperty<T> : PropProperty
{
    public T Value;

    public PropProperty(string name, T defaultValue) : base(name)
    {
        Value = defaultValue;
    }

    public PropProperty<T> Set(T value)
    {
        Value = value;
        return this;
    }
    
    public T Get()
    {
        return Value;
    }
}