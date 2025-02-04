using SoulEngine.SequenceScript.Lexing;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

public class UnaryExpressionNode : ExpressionNode
{
    public ExpressionNode Value;
    public Token Operation;
}