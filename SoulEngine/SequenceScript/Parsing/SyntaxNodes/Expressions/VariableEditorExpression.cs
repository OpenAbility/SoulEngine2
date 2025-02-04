using SoulEngine.SequenceScript.Lexing;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes.Expressions;

public class VariableEditorExpression : ExpressionNode
{
    public Token Variable;
    public Token Operator;
    public ExpressionNode Expression;
}