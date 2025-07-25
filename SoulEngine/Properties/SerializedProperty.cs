using SoulEngine.Data.NBT;

namespace SoulEngine;

public abstract class SerializedProperty
{
    public readonly string Name;

    public SerializedProperty(string name)
    {
        Name = name;
    }
    
    
    public abstract void Edit();

    public abstract void MakeCurrentReset();
    public abstract void Reset();
    public abstract Tag Save();
    public abstract void Load(Tag tag);
}

public abstract class SerializedProperty<T> : SerializedProperty
{
    public T Value;
    private T resetValue;

    protected SerializedProperty(string name, T defaultValue) : base(name)
    {
        Value = defaultValue;
        resetValue = defaultValue;
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

    public override void MakeCurrentReset()
    {
        resetValue = Get();
    }

    public override void Reset()
    {
        Set(resetValue);
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SerializedPropertyAttribute(string id) : Attribute
{
    public readonly string ID = id;
}