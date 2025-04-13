using SoulEngine.SequenceScript.Compile;
using SoulEngine.SequenceScript.Lexing;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

public class VariableEditorExpression : ExpressionNode
{
    public Token Variable;
    public Token Operator;
    public ExpressionNode Expression;
    public override CodeLocation GetLocation() => Variable.Location;
}