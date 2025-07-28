using SoulEngine.SequenceScript.Compile;
using SoulEngine.SequenceScript.Lexing;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

public class BinaryExpressionNode : ExpressionNode
{
    public ExpressionNode Left;
    public Token Operand;
    public ExpressionNode Right;

    public BinaryExpressionNode(ExpressionNode left, Token operand, ExpressionNode right)
    {
        Left = left;
        Operand = operand;
        Right = right;
    }

    public override CodeLocation GetLocation()
    {
        return Left.GetLocation();
    }
}