using SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes;

public class SwitchStatement : SyntaxNode
{
    public ExpressionNode Expression;
    public SwitchCase[] Cases;
    public BodyNode? Default;
}

public struct SwitchCase
{
    public ExpressionNode Expression;
    public BodyNode Body;
}