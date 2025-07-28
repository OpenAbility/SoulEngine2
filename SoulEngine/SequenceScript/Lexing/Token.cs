using SoulEngine.SequenceScript.Compile;

namespace SoulEngine.SequenceScript.Lexing;

public struct Token
{
    public CodeLocation Location;
    public TokenType TokenType;
    public string Value;

    public bool WhitespaceFollowed;

    public Token(CodeLocation location, TokenType tokenType, string value, bool whitespaceFollowed)
    {
        Location = location;
        TokenType = tokenType;
        Value = value;
        WhitespaceFollowed = whitespaceFollowed;
    }
    
    public Token(CodeLocation location, TokenType tokenType, bool whitespaceFollowed)
    {
        Location = location;
        TokenType = tokenType;
        Value = "";
        WhitespaceFollowed = whitespaceFollowed;
    }

    public override string ToString()
    {
	    return $"Token {{ {Location}: {TokenType}: '{Value}' (WS: {WhitespaceFollowed}) }}";
    }
}

public enum TokenType
{
	Unknown,
	EOF,
	
	Identifier,
	Numeric,
	String,
	EndStatement,

	OpenParenthesis,	// (
	CloseParenthesis,	// )
	OpenBraces,			// {
	CloseBraces,		// }
	OpenBrackets,		// [
	CloseBrackets,		// ]
	
	Comma,
	Colon,
	
	Star,
	Slash,
	Modulus,
	Plus,
	Minus,
	
	Equals,
	NotEquals,
	LessThan,
	GreaterThan,
	LessEquals,
	GreaterEquals,
	
	And,
	Or,
	Not,
	
	Increment,
	Decrement,
	
	Assign, // =
	
	// Keywords
	ImportKw,
	SwitchKw,
	CaseKw,
	DefaultKw,
	
	ReturnKw,
	ContinueKw,
	BreakKw,
	
	IfKw,
	ElseKw,
	
	ForKw,
	WhileKw,
	
	TrueKw,
	FalseKw,
	
	NullKw,
	
	VoidKw,
	IntKw,
	FloatKw,
	BoolKw,
	StringKw,
	HandleKw,
	
	GlobalKw,
	ConstKw,
	ExternKw,
	
	ProcKw,
	
	MetaCharacter
}