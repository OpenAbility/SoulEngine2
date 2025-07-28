using SoulEngine.SequenceScript.Compile;
using SoulEngine.SequenceScript.Lexing;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

public class ConstantNode : ExpressionNode
{
    public Token Value;

    public ConstantNode(Token value)
    {
        Value = value;
    }

    public ConstantNode()
    {
        
    }

    public override CodeLocation GetLocation() => Value.Location;
}