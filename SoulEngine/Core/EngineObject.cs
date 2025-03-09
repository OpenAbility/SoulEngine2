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
    
}