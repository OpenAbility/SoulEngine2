using SoulEngine.SequenceScript.Lexing;
using SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes;

public class GlobalStatement : SyntaxNode
{
    public Token Identifier;
    public Token Type;
    public ExpressionNode? DefaultValue;
}