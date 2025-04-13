namespace SoulEngine.SequenceScript.Emitter;

public class VariablePrototype : IdentifiablePrototype
{

    public readonly Machine.ValueType Type;
    public readonly bool IsLocal;

    public VariablePrototype(string name, string packageName, Machine.ValueType type, bool isLocal) : base(name, packageName)
    {
        Type = type;
        IsLocal = isLocal;
    }
}