using SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes;

public class WhileStatement : ExpressionNode
{
    public ExpressionNode Comparison;
    public BodyNode Body;
}