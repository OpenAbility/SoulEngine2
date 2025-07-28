using SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes;

public class ReturnStatement : SyntaxNode
{
    public ExpressionNode? ExpressionNode;
}