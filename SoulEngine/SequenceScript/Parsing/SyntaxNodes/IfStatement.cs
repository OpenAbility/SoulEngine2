using SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes;

public class IfStatement : SyntaxNode
{
    public ExpressionNode? Expression;
    public BodyNode Body = null!;
    public IfStatement? Next;
}