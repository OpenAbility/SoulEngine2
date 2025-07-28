using SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes;

public class SwitchStatement : SyntaxNode
{
    public ExpressionNode Expression = null!;
    public SwitchCase[] Cases = null!;
    public BodyNode? Default;
}

public struct SwitchCase
{
    public ExpressionNode Expression;
    public BodyNode Body;
}