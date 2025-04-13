using SoulEngine.SequenceScript.Compile;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

public abstract class ExpressionNode : SyntaxNode
{
    public abstract CodeLocation GetLocation();
}