using SoulEngine.SequenceScript.Compile;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

public class BogusExpression : ExpressionNode
{

    private readonly CodeLocation location;

    public BogusExpression(CodeLocation location)
    {
        this.location = location;
    }
    
    public override CodeLocation GetLocation()
    {
        return location;
    }
}