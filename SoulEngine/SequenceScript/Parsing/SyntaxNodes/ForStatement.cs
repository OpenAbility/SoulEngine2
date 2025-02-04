using SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes;

public class ForStatement : SyntaxNode
{
    public SyntaxNode Initializer;
    public ExpressionNode Comparison;
    public SyntaxNode Incrementor;

    public BodyNode Body;
}