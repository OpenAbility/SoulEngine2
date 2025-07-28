using SoulEngine.SequenceScript.Compile;
using SoulEngine.SequenceScript.Lexing;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

public class ProcedureCallExpressionNode : ExpressionNode
{
    public Token Identifier;
    public ExpressionNode[] Parameters = null!;
    public override CodeLocation GetLocation() => Identifier.Location;
}