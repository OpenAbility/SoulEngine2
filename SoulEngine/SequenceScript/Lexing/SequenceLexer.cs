using System.Text;
using SoulEngine.SequenceScript.Compile;
using SoulEngine.SequenceScript.Utility;

namespace SoulEngine.SequenceScript.Lexing;

// This could've been done using regex but fuck it.

public class SequenceLexer
{
    private readonly LexicalReader reader;
    private readonly List<Token> tokens = new List<Token>();
    private readonly CompilerContext context;
    
    public SequenceLexer(string path, Stream stream, Encoding encoding, CompilerContext context)
    {
        reader = new LexicalReader(stream, encoding, path);
        this.context = context;
    }

    public Token[] GetTokens() => tokens.ToArray();

    public void Process()
    {
        while (!reader.EOF)
        {
            
            if(reader.Current == '\n' || reader.Current == '\t' || reader.Current == '\r' || reader.Current == ' ' || reader.Current == '\t' || reader.Current == '\0')
                reader.Step();
            else if (reader.Current == '/' && reader.Peek(1) == '/')
                ParseCommentOneLine();
            else if (reader.Current == '/' && reader.Peek(1) == '*')
                ParseCommentMultiline();
            
			else if(TryConsumePush(';', TokenType.EndStatement)) continue;
			else if(TryConsumePush(',', TokenType.Comma)) continue;
				
			else if(TryConsumePush('(', TokenType.OpenParenthesis)) continue;
			else if(TryConsumePush(')', TokenType.CloseParenthesis)) continue;
			else if(TryConsumePush('[', TokenType.OpenBrackets)) continue;
			else if(TryConsumePush(']', TokenType.CloseBrackets)) continue;
			else if(TryConsumePush('{', TokenType.OpenBraces)) continue;
			else if(TryConsumePush('}', TokenType.CloseBraces)) continue;
			
			else if(TryConsumePush('}', TokenType.CloseBraces)) continue;
			
			else if(TryConsumePush("++", TokenType.Increment)) continue;
			else if(TryConsumePush("--", TokenType.Decrement)) continue;
			
			else if(TryConsumePush("==", TokenType.Equals)) continue;
			else if(TryConsumePush("!=", TokenType.NotEquals)) continue;
			else if(TryConsumePush(">=", TokenType.GreaterEquals)) continue;
			else if(TryConsumePush("<=", TokenType.LessEquals)) continue;
			
			else if(TryConsumePush('+', TokenType.Plus)) continue;
			else if(TryConsumePush('-', TokenType.Minus)) continue;
			else if(TryConsumePush('*', TokenType.Star)) continue;
			else if(TryConsumePush('/', TokenType.Slash)) continue;
			else if(TryConsumePush('%', TokenType.Modulus)) continue;
			
			else if(TryConsumePush("&&", TokenType.And)) continue;
			else if(TryConsumePush("||", TokenType.Or)) continue;
			
			else if(TryConsumePush('!', TokenType.Not)) continue;
			
			else if(TryConsumePush('<', TokenType.LessThan)) continue;
			else if(TryConsumePush('>', TokenType.GreaterThan)) continue;
			
			else if(TryConsumePush('=', TokenType.Assign)) continue;
			else if(TryConsumePush(':', TokenType.Colon)) continue;
            else if(TryConsumePush('#', TokenType.MetaCharacter)) continue;
            
            else if (SequenceFormatting.IsValidIdentifier(reader.Current, true)) ParseIdentifier();
            else if (reader.Current == '"') ParseString();
            
            // Numerics
            else if (char.IsAsciiDigit(reader.Current)) ParseNumeric();
            else if (reader.Current == '.' && char.IsAsciiDigit(reader.Peek(1))) ParseNumeric();
            
            else
            {
                context.Error(reader.Location, "SS1001",
                    "Unexpected character '" + SequenceFormatting.FormatCharacter(reader.Current) + "'");
                reader.Step();
            }
        }
    }

    private bool TryConsumePush(char c, TokenType type)
    {
        if (reader.Current != c)
            return false;
        
        tokens.Add(new Token(reader.Location, type, c.ToString(), char.IsWhiteSpace(reader.Peek(1))));
        
        reader.Step(1);
        return true;
    }

    private bool TryConsumePush(string s, TokenType type)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (reader.Peek(i) != s[i])
                return false;
        }
        
        tokens.Add(new Token(reader.Location, type, s, char.IsWhiteSpace(reader.Peek(1))));
        
        reader.Step(s.Length);
        return true;
    }

    private void ParseIdentifier()
    {
	    StringBuilder builder = new StringBuilder();

	    CodeLocation location = reader.Location;
	    
	    while (SequenceFormatting.IsValidIdentifier(reader.Current, false))
	    {
		    builder.Append(reader.Current);
		    reader.Step();
	    }
	    
	    tokens.Add(new Token(location, RecognizeIdentifier(builder.ToString()), builder.ToString(), char.IsWhiteSpace(reader.Current)));
    }

    private void ParseString()
    {
	    StringBuilder builder = new StringBuilder();

	    CodeLocation location = reader.Location;
	    
	    bool escaped = false;
	    reader.Step(1);

	    while (true)
	    {
		    if (reader.EOF)
		    {
			    context.Error(reader.Location, "SS1000", "Reached end of file whilst parsing string!");
			    break;
		    }
		    
		    if (escaped)
		    {
			    builder.Append(SequenceFormatting.GetEscaped(reader.Location, reader.Current, context));
			    escaped = false;
		    } else if (reader.Current == '"')
		    {
			    reader.Step(1);
			    break;
		    }
		    else
		    {
			    builder.Append(reader.Current);
		    }

		    reader.Step(1);
	    }
	    
	    tokens.Add(new Token(location, TokenType.String, builder.ToString(), char.IsWhiteSpace(reader.Current)));
    }

    private void ParseNumeric()
    {
	    StringBuilder builder = new StringBuilder();
	    bool hasDecimal = false;

	    CodeLocation location = reader.Location;

	    while (true)
	    {
		    if (reader.Current == '.' && !hasDecimal)
		    {
			    builder.Append('.');
			    reader.Step();
			    hasDecimal = true;
		    } else if (char.IsAsciiDigit(reader.Current))
		    {
			    builder.Append(reader.Current);
			    reader.Step();
		    } else if (reader.Current == '_') // For formatting
		    {
			    reader.Step();
		    }
		    else
		    {
			    break;
		    }
	    }

	    if (reader.Current == 'f' && !hasDecimal)
	    {
		    builder.Append(".0");
		    reader.Step();
	    }
	    
	    	    
	    tokens.Add(new Token(location, TokenType.Numeric, builder.ToString(), char.IsWhiteSpace(reader.Current)));
	    
    }
    
    private void ParseCommentOneLine()
    {
        reader.Step(2);

        while (!reader.EOF && reader.Current != '\n')
            reader.Step();
    }
    
    private void ParseCommentMultiline()
    {
        reader.Step(2);

        while (!reader.EOF && !(reader.Current == '*' && reader.Peek(1) == '/'))
            reader.Step();
        
        reader.Step(2);
    }
    
    private TokenType RecognizeIdentifier(string s)
    {
	    return s switch
	    {
		    "import" => TokenType.ImportKw,
		    "switch" => TokenType.SwitchKw,
		    "case" => TokenType.CaseKw,
		    "default" => TokenType.DefaultKw,
			
		    "break" => TokenType.BreakKw,
		    "return" => TokenType.ReturnKw,
		    "continue" => TokenType.ContinueKw,
			
		    "if" => TokenType.IfKw,
		    "else" => TokenType.ElseKw,
			
		    "for" => TokenType.ForKw,
		    "while" => TokenType.WhileKw,
			
		    "void" => TokenType.VoidKw,
		    "int" => TokenType.IntKw,
		    "float" => TokenType.FloatKw,
		    "bool" => TokenType.BoolKw,
		    "string" => TokenType.StringKw,
		    "handle" => TokenType.HandleKw,
			
		    "true" => TokenType.TrueKw,
		    "false" => TokenType.FalseKw,
			
		    "global" => TokenType.GlobalKw,
		    "const" => TokenType.ConstKw,
		    "extern" => TokenType.ExternKw,
		    
		    "proc" => TokenType.ProcKw,
		    
		    "null" => TokenType.NullKw,
			
		    _ => TokenType.Identifier
	    };
    }
}