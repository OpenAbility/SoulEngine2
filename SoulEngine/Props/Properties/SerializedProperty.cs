using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public abstract class SerializedProperty
{
    public readonly string Name;

    public SerializedProperty(string name)
    {
        Name = name;
    }
    
    
    public abstract void Edit();
    
    
    
    public abstract Tag Save();
    public abstract void Load(Tag tag);
}

public abstract class SerializedProperty<T> : SerializedProperty
{
    public T Value;
    private T defaultValue;

    public SerializedProperty(string name, T defaultValue) : base(name)
    {
        Value = defaultValue;
    }

    public SerializedProperty<T> Set(T value)
    {
        Value = value;
        return this;
    }
    
    public T Get()
    {
        return Value;
    }
}