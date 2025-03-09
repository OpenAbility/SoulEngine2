namespace SoulEngine.SequenceScript.Emitter;

public class VariablePrototype : IdentifiablePrototype
{

    public readonly ValueType Type;
    
    public VariablePrototype(string name, string packageName, ValueType type) : base(name, packageName)
    {
        Type = type;
    }
}