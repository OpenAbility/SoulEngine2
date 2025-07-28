using SoulEngine.Data.NBT;

namespace SoulEngine.Core;

public abstract class EngineObject
{
    private static ulong currentID;
    
    public readonly ulong ObjectID;

    public EngineObject()
    {
        ObjectID = currentID++;
    }

    public virtual void Edit()
    {
        
    }

    public virtual void Load(CompoundTag tag)
    {
        
    }

    public virtual CompoundTag Save()
    {
        return new CompoundTag(null!);
    }

}