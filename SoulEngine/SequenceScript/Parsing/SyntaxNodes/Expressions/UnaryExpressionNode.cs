using SoulEngine.SequenceScript.Compile;
using SoulEngine.SequenceScript.Lexing;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

public class UnaryExpressionNode : ExpressionNode
{
    public ExpressionNode Value = null!;
    public Token Operation;
    public override CodeLocation GetLocation() => Value.GetLocation();
}