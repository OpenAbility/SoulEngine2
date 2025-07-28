using SoulEngine.SequenceScript.Lexing;
using SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes;

public class LocalVariableDefinition : SyntaxNode
{
    public Token Identifier;
    public Token Type;
    public bool IsArray;

    public ExpressionNode? DefaultValue;
}