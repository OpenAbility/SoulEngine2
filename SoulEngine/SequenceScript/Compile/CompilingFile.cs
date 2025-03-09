using SoulEngine.SequenceScript.Lexing;
using SoulEngine.SequenceScript.Parsing.SyntaxNodes;
using ValueType = SoulEngine.SequenceScript.Machine.ValueType;

namespace SoulEngine.SequenceScript.Compile;

public class CompilingFile
{
    public Token[] Tokens;
    public ProgramRootNode AST;

    public string ResolvePath;
    public string InputPath;
    public string OutputPath;

    public readonly Dictionary<string, CompilingFunction> functions = new Dictionary<string, CompilingFunction>();
    public readonly Dictionary<string, ValueType> globals = new Dictionary<string, ValueType>();
}

public struct CompilingFunction
{
    public string Name;
    public ValueType? ReturnType;
    public ValueType[] ParameterTypes;
}