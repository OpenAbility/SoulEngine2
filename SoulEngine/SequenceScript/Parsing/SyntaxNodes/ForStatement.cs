using SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes;

public class ForStatement : SyntaxNode
{
    public SyntaxNode Initializer = null!;
    public ExpressionNode Comparison = null!;
    public SyntaxNode Incrementor = null!;

    public BodyNode Body = null!;
}