using ValueType = SoulEngine.SequenceScript.Machine.ValueType;

namespace SoulEngine.SequenceScript.Emitter;

public struct Scope()
{

    public readonly Dictionary<string, IdentifiablePrototype> Prototypes =
        new Dictionary<string, IdentifiablePrototype>();

    public ValueType? ReturnType;

    public Scope Clone()
    {
        Scope newScope = new Scope();

        foreach (var kvp in Prototypes)
        {
            newScope.Prototypes[kvp.Key] = kvp.Value;
        }

        return newScope;
    }
    
}