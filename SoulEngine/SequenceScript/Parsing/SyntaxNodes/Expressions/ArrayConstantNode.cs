using SoulEngine.SequenceScript.Lexing;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

public class ArrayConstantNode : ExpressionNode
{
    public ExpressionNode[] Values;
}