using SoulEngine.SequenceScript.Lexing;
using ValueType = SoulEngine.SequenceScript.Machine.ValueType;

namespace SoulEngine.SequenceScript.Utility;

public static class SequenceRules
{
    public static int GetUnaryOperatorPrecedence(TokenType type)
    {
        return type switch
        {
            TokenType.Minus => 6,
            TokenType.Plus => 6,
            TokenType.Not => 6,
			
            _ => 0
        };
    }

    public static ValueType KeywordToValueType(TokenType type)
    {
        return type switch
        {
            TokenType.StringKw => ValueType.String,
            TokenType.IntKw => ValueType.Integer,
            TokenType.BoolKw => ValueType.Boolean,
            TokenType.FloatKw => ValueType.Floating,
            TokenType.HandleKw => ValueType.Handle,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    public static ValueType? KeywordToReturnType(TokenType type)
    {
        return type switch
        {
            TokenType.StringKw => ValueType.String,
            TokenType.IntKw => ValueType.Integer,
            TokenType.BoolKw => ValueType.Boolean,
            TokenType.FloatKw => ValueType.Floating,
            TokenType.HandleKw => ValueType.Handle,
            TokenType.VoidKw => null,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static int GetBinaryOperatorPrecedence(TokenType type)
    {
        return type switch
        {
            TokenType.Increment => 6,
            TokenType.Decrement => 6,
			
            TokenType.Star => 5,
            TokenType.Slash => 5,
            TokenType.Modulus => 5,
			
            TokenType.Plus => 4,
            TokenType.Minus => 4,
			
            TokenType.Equals => 3,
            TokenType.NotEquals => 3,
            TokenType.LessThan => 3,
            TokenType.GreaterThan => 3,
            TokenType.LessEquals => 3,
            TokenType.GreaterEquals => 3,
			
            TokenType.And => 2,
			
            TokenType.Or => 1,
			
            _ => 0
        };
    }

    public static readonly TokenType[] AssignableTokenTypes =
    [
        TokenType.IntKw,
        TokenType.FloatKw,
        TokenType.StringKw,
        TokenType.HandleKw,
        TokenType.BoolKw
    ];
}