using SoulEngine.SequenceScript.Compile;
using SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes;

public class WhileStatement : SyntaxNode
{
    public ExpressionNode Comparison;
    public BodyNode Body;
}