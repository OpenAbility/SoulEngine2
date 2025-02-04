using SoulEngine.SequenceScript.Lexing;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

public class VariableAssignationExpression : ExpressionNode
{
    public Token Variable;
    public ExpressionNode Value;
}