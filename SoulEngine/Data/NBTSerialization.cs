using System.Reflection;
using SoulEngine.Core;
using SoulEngine.Data.NBT;
using SoulEngine.Util;

namespace SoulEngine.Data;

public static class NBTSerialization
{

    private static readonly Dictionary<Type, INBTSerializer> serializers = new Dictionary<Type, INBTSerializer>();
    
    static NBTSerialization()
    {
        foreach (var type in EngineUtility.GetAllTypeAttributes<NBTSerializerAttribute>())
        {
            if(!type.Type.IsAssignableTo(typeof(INBTSerializer)))
                continue;
            
            if(!type.Type.CanInstance())
                continue;

            foreach (NBTSerializerAttribute attribute in type.Type.GetCustomAttributes<NBTSerializerAttribute>())
            {
                serializers[attribute.Type] = type.Type.Instantiate<INBTSerializer>()!;
            }
        }
    }

    private static INBTSerializer GetSerializer(Type type)
    {
        if(serializers.TryGetValue(type, out INBTSerializer? serializer))
            return serializer;
        
        int lowest = Int32.MaxValue;
        foreach (var existing in serializers)
        {
            int level = EngineUtility.GetInheritanceLevel(type, existing.Key);
            if(level == -1)
                continue;

            if (level < lowest)
            {
                lowest = level;
                serializer = existing.Value;
            }
        }

        if (serializer == null)
            throw new Exception("Unable to fit existing serializer to type " + type);

        serializers[type] = serializer;
        return serializer;
    }
    
    public static Tag? Serialize(object o)
    {
        return GetSerializer(o.GetType()).Serialize(o, new NBTSerializationContext(null, o.GetType()));
    }
    
    public static T? Deserialize<T>(Tag tag)
    {
        return (T?)GetSerializer(typeof(T)).Deserialize(tag, new NBTSerializationContext(null, typeof(T)));
    }
    
    public static object? Deserialize(Tag tag, Type type)
    {
        return GetSerializer(type).Deserialize(tag, new NBTSerializationContext(null, type));
    }
    
    public static object? Deserialize(Tag tag, Type type, Scene scene)
    {
        return GetSerializer(type).Deserialize(tag, new NBTSerializationContext(scene, type));
    }
    
    public static Tag? Serialize(object o, Scene scene)
    {
        return GetSerializer(o.GetType()).Serialize(o, new NBTSerializationContext(scene, o.GetType()));
    }
}