using SoulEngine.SequenceScript.Lexing;
using SoulEngine.SequenceScript.Machine;
using ValueType = SoulEngine.SequenceScript.Machine.ValueType;

namespace SoulEngine.SequenceScript.Utility;

public static class SequenceRules
{
    
    public static Version Version = new Version(1, 0, 0);
    
    public static int GetUnaryOperatorPrecedence(TokenType type)
    {
        return type switch
        {
            TokenType.Minus => 6,
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

    public static ValueType TokenToValueType(Token token)
    {
        if (token.TokenType == TokenType.String)
            return ValueType.String;
        if (token.TokenType == TokenType.TrueKw ||
            token.TokenType == TokenType.FalseKw)
            return ValueType.Boolean;
        if (token.TokenType == TokenType.NullKw)
            return ValueType.Handle;

        if (token.TokenType == TokenType.Numeric)
        {
            if (token.Value.Contains('.'))
                return ValueType.Floating;
            return ValueType.Integer;
        }

        return ValueType.Bogus;
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

    public static bool IsNumericType(this ValueType? valueType)
    {
        return valueType switch
        {
            ValueType.Floating => true,
            ValueType.Integer => true,
            _ => false
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

    public static ValueType? GetOperandType(OpCode opCode)
    {
        return opCode switch
        {
            OpCode.PUSHI => ValueType.Integer,
            OpCode.PUSHF => ValueType.Floating,
            OpCode.PUSHB => ValueType.Boolean,
            OpCode.PUSHS => ValueType.String,
            OpCode.PUSHN => null,
            OpCode.POP => null,
            OpCode.CLONE => null,
            OpCode.STORE => ValueType.String,
            OpCode.LOAD => ValueType.String,
            OpCode.ADD => null,
            OpCode.SUB => null,
            OpCode.MULT => null,
            OpCode.DIV => null,
            OpCode.MOD => null,
            OpCode.XOR => null,
            OpCode.AND => null,
            OpCode.OR => null,
            OpCode.NOT => null,
            OpCode.IGT => null,
            OpCode.ILT => null,
            OpCode.IEQ => null,
            OpCode.JMP => ValueType.Integer,
            OpCode.CALL => null,
            OpCode.RET => null,
            OpCode.INT => null,
            _ => throw new ArgumentOutOfRangeException(nameof(opCode), opCode, null)
        };
    }
}