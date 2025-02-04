using SoulEngine.SequenceScript.Lexing;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

public class VariableExpression : ExpressionNode
{
    public Token Value;

    public VariableExpression(Token value)
    {
        Value = value;
    }

    public VariableExpression()
    {
        
    }
}