using SoulEngine.Core;

namespace SoulEngine.Data;

public class NBTSerializationContext
{
    public readonly Scene? Scene;
    public readonly Type Type;

    public NBTSerializationContext(Scene? scene, Type type)
    {
        Scene = scene;
        Type = type;
    }
}