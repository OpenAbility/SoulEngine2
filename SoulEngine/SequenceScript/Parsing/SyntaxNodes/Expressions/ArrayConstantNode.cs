using SoulEngine.SequenceScript.Compile;
using SoulEngine.SequenceScript.Lexing;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

public class ArrayConstantNode : ExpressionNode
{
    public ExpressionNode[] Values;
    public CodeLocation Location;
    
    public override CodeLocation GetLocation()
    {
        return Location;
    }
}