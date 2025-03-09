using SoulEngine.SequenceScript.Compile;

namespace SoulEngine.SequenceScript.Emitter;

public class FunctionPrototype : IdentifiablePrototype
{

    public readonly CompilingFunction Underlying;
    
    public FunctionPrototype(string name, string packageName, CompilingFunction function) : base(name, packageName)
    {
        Underlying = function;
    }
}