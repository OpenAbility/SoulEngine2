using SoulEngine.Data.NBT;

namespace SoulEngine.Data;

public interface INBTSerializer
{
    public Tag? Serialize(object value, NBTSerializationContext context);
    public object? Deserialize(Tag tag, NBTSerializationContext context);
}

public interface INBTSerializer<T> : INBTSerializer
{
    Tag? INBTSerializer.Serialize(object value, NBTSerializationContext context)
    {
        if(value is T instance)
            return Serialize(instance, context);
        return null;
    }
    
    object? INBTSerializer.Deserialize(Tag tag, NBTSerializationContext context)
    {
        return Deserialize(tag, context);
    }
    
    public Tag Serialize(T value, NBTSerializationContext context);
    public T? Deserialize(Tag tag, NBTSerializationContext context);
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public class NBTSerializerAttribute(Type type) : Attribute
{
    public readonly Type Type = type;
}