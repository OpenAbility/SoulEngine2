using SoulEngine.SequenceScript.Lexing;
using SoulEngine.SequenceScript.Parsing.SyntaxNodes;
using ValueType = SoulEngine.SequenceScript.Machine.ValueType;

namespace SoulEngine.SequenceScript.Compile;

public class CompilingFile
{
    public Token[] Tokens = null!;
    public ProgramRootNode AST  = null!;

    public string ResolvePath = null!;
    public string InputPath = null!;
    public string OutputPath = null!;

    public readonly Dictionary<string, CompilingFunction> Functions = new Dictionary<string, CompilingFunction>();
    public readonly Dictionary<string, ValueType> Globals = new Dictionary<string, ValueType>();
}

public struct CompilingFunction
{
    public string Name;
    public ValueType? ReturnType;
    public ValueType[] ParameterTypes;
    public bool SystemFunction;
}