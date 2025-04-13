using SoulEngine.SequenceScript.Compile;
using SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes;

public class BogusStatement : ExpressionNode
{
    
    
    private readonly CodeLocation location;

    public BogusStatement(CodeLocation location)
    {
        this.location = location;
    }
    
    public override CodeLocation GetLocation()
    {
        return location;
    }
}