using SoulEngine.SequenceScript.Lexing;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes;

public class ProcedureDefinitionNode : SyntaxNode
{
    public Token ReturnType;
    public Token Identifier;

    public ParameterDefinitionNode[] Parameters;

    public BodyNode Body;
}

public class ParameterDefinitionNode : SyntaxNode
{
    public bool Out;
    
    public Token Type;
    public Token Identifier;
}