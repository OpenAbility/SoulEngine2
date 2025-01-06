using System.Text;

namespace SoulEngine.Data.NBT;

public static class TagParser
{
    public static Tag Parse(string s)
    {
        Lexer lexer = new Lexer(s);
        lexer.Process();

        Parser parser = new Parser(lexer.Tokens);
        return parser.ParseNamedTag();
    }

    private class Parser
    {
        public Parser(List<Token> buffer)
        {
            this.buffer = buffer;
        }

        private readonly List<Token> buffer;

        private int index = 0;
        private Token Current => Peek(0);

        private Token Peek(int amt)
        {
            if (index + amt >= buffer.Count)
                return new Token();
            return buffer[index + amt];
        }

        private void Step(int amt = 1)
        {
            index += amt;
        }
        
        private Token Consume(params TokenType[] tokenType)
        {
            if(!tokenType.Contains(Current.Type))
                throw new Exception("Token was not inside array [" + string.Join(", ", tokenType) + "]");
            Token c = Current;
            Step();
            return c;
        }

        private void ConsumeNumericExt(string expected)
        {
            if(Current.Type != TokenType.Symbol)
                return;

            string c = Consume(TokenType.Symbol).Value;
            
            if (!string.Equals(c, expected, StringComparison.InvariantCultureIgnoreCase))
                throw new Exception("Expected numeric type '" + c.ToLowerInvariant() + "', only '" + expected.ToLowerInvariant() +
                                    "' was allowed!");
        }

        public Tag ParseNamedTag()
        {
            string name = Consume(TokenType.String, TokenType.Symbol).Value;
            Consume(TokenType.Colon);
            
            Tag tag = ParseTag();
            tag.Name = name;
            return tag;
        }

        private Tag ParseCompound()
        {
            Consume(TokenType.CompoundOpener);

            CompoundTag compoundTag = new CompoundTag(null);

            while (Current.Type != TokenType.CompoundCloser)
            {
                compoundTag.Add(ParseNamedTag());

                if (Current.Type == TokenType.ListDelimiter)
                {
                    Consume(TokenType.ListDelimiter);
                }
                else
                {
                    break;
                }
            }

            Consume(TokenType.CompoundCloser);

            return compoundTag;
        }

        private string[] numericFollowers =
        [
            "b",
            "s",
            "l",
            "f",
            "d"
        ];

        private Tag ParseComplexNumeric(string value, string follower)
        {
            if (follower == "b")
                return new ByteTag(null, Byte.Parse(value));
            if (follower == "s")
                return new ShortTag(null, Int16.Parse(value));
            if (follower == "l")
                return new LongTag(null, Int64.Parse(value));
            if (follower == "f")
                return new FloatTag(null, Single.Parse(value));
            if (follower == "d")
                return new DoubleTag(null, Double.Parse(value));
            throw new Exception("Somehow parsed invalid complex numeric!");
        }
        
        private Tag ParseNumeric()
        {
            string value = Consume(TokenType.Numeric).Value;

            if (Current.Type == TokenType.Symbol && numericFollowers.Contains(Current.Value.ToLowerInvariant()))
            {
                return ParseComplexNumeric(value, Consume(TokenType.Symbol).Value.ToLowerInvariant());
            }

            if (value.Contains('.'))
                return new DoubleTag(null, Double.Parse(value));
            return new IntTag(null, Int32.Parse(value));
        }

        private Tag ParseByteArray()
        {
            List<byte> values = new List<byte>();
            while (true)
            {
                values.Add(Byte.Parse(Consume(TokenType.Numeric).Value));
                ConsumeNumericExt("b");

                if (Current.Type == TokenType.ListDelimiter)
                {
                    Consume(TokenType.ListDelimiter);
                }
                else
                {
                    break;
                }
            }

            return new ByteArrayTag(null, values.ToArray());
        }
        
        private Tag ParseIntArray()
        {
            List<int> values = new List<int>();
            while (true)
            {
                values.Add(Byte.Parse(Consume(TokenType.Numeric).Value));

                if (Current.Type == TokenType.ListDelimiter)
                {
                    Consume(TokenType.ListDelimiter);
                }
                else
                {
                    break;
                }
            }

            return new IntArrayTag(null, values.ToArray());
        }
        
        private Tag ParseLongArray()
        {
            List<long> values = new List<long>();
            while (true)
            {
                values.Add(Byte.Parse(Consume(TokenType.Numeric).Value));
                ConsumeNumericExt("l");

                if (Current.Type == TokenType.ListDelimiter)
                {
                    Consume(TokenType.ListDelimiter);
                }
                else
                {
                    break;
                }
            }

            return new LongArrayTag(null, values.ToArray());
        }
        
        private Tag ParseArray()
        {

            string type = Consume(TokenType.Symbol).Value;
            Consume(TokenType.ArrayDelimiter);

            Tag t;
            if (type == "B")
                t = ParseByteArray();
            else
                throw new Exception("Invalid array type '" + type + "'!");

            Consume(TokenType.ArrayCloser);
            return t;
        }

        private Tag ParseList()
        {
            Consume(TokenType.ArrayOpener);

            if (Current.Type == TokenType.Symbol)
                return ParseArray();

            ListTag listTag = new ListTag(null);
            
            while (true)
            {
                listTag.Add(ParseTag());
                if (Current.Type == TokenType.ListDelimiter)
                {
                    Consume(TokenType.ListDelimiter);
                }
                else
                {
                    break;
                }
            }
            
            Consume(TokenType.ArrayCloser);

            return listTag;

        }

        public Tag ParseTag()
        {
            if (Current.Type == TokenType.String) return new StringTag(null, Consume(TokenType.String).Value);
            if (Current.Type == TokenType.CompoundOpener) return ParseCompound();
            if (Current.Type == TokenType.Numeric) return ParseNumeric();
            if (Current.Type == TokenType.Symbol && Current.Value is "true" or "false")
            {
                return new ByteTag(null, Consume(TokenType.Symbol).Value == "true" ? (byte)1 : (byte)0);
            }

            if (Current.Type == TokenType.ArrayOpener) return ParseList();

            throw new Exception("Unexpected tag token " + Current.Type);
        }
    }

    private class Lexer
    {

        public Lexer(string buffer)
        {
            this.buffer = buffer;
        }

        public readonly List<Token> Tokens = new List<Token>();
        private readonly string buffer;

        private int index = 0;
        private char Current => Peek(0);

        private char Peek(int amt)
        {
            if (index + amt >= buffer.Length)
                return '\0';
            return buffer[index + amt];
        }

        private void Step(int amt = 1)
        {
            index++;
        }

        private void PushAndStep(TokenType tokenType)
        {
            Tokens.Add(new Token(tokenType, Current.ToString()));
            Step();
        }

        private bool IsValidId(char c)
        {
            return char.IsAsciiLetterOrDigit(c) || c == '_' || c == '-' || c == '.' || c == '+';
        }

        private static readonly Dictionary<char, char> EscapeCodes = new Dictionary<char, char>()
        {
            {'n', '\n'},
            {'\\', '\\'},
            {'"', '"'},
            {'\'', '\''}
        };

        private void LexString()
        {
            char delim = Current;
            Step();

            StringBuilder builder = new StringBuilder();
            bool escaped = false;

            while (Current != delim)
            {
                if (Current == '\0')
                    throw new Exception("String EOF");

                if (escaped)
                {
                    builder.Append(EscapeCodes[Current]);
                    escaped = false;
                } else if (Current == '\\')
                {
                    escaped = true;
                }
                else
                {
                    builder.Append(Current);
                }
                
                Step();

            }
            
            Tokens.Add(new Token(TokenType.String, builder.ToString()));
            Step();
        }
        
        private void LexID()
        {
            StringBuilder builder = new StringBuilder();
            bool escaped = false;

            while (IsValidId(Current))
            {
                builder.Append(Current);
                Step();
            }
            
            Tokens.Add(new Token(TokenType.Symbol, builder.ToString()));
        }
        
        private void LexNumeric()
        {
            StringBuilder builder = new StringBuilder();
            bool escaped = false;

            bool hasDecimal = false;

            // Allow for one decimal point
            while (char.IsAsciiDigit(Current) || (Current == '.' && !hasDecimal))
            {
                if (Current == '.')
                    hasDecimal = true;
                
                builder.Append(Current);
                Step();
            }
            
            Tokens.Add(new Token(TokenType.Numeric, builder.ToString()));
        }

        public void Process()
        {

            while (Current != '\0')
            {
                if(char.IsWhiteSpace(Current)) Step();
                else if (Current == '[') PushAndStep(TokenType.ArrayOpener);
                else if (Current == ']') PushAndStep(TokenType.ArrayCloser);
                else if (Current == ';') PushAndStep(TokenType.ArrayDelimiter);
                else if (Current == ':') PushAndStep(TokenType.Colon);
                else if (Current == ',') PushAndStep(TokenType.ListDelimiter);
                else if (Current == '{') PushAndStep(TokenType.CompoundOpener);
                else if (Current == '}') PushAndStep(TokenType.CompoundCloser);
                else if (Current == '"' || Current == '\'') LexString();
                else if (char.IsAsciiDigit(Current)) LexNumeric();
                else if (IsValidId(Current)) LexID();
                else
                {
                    throw new Exception("Malformed SNBT data!");
                }
            }
            
        }
        
        

    }
    
    private struct Token
    {
        public TokenType Type;
        public string Value;

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }
    }
    
    private enum TokenType
    {
        String, // "hi"
        Numeric, // 1234
        
        Symbol, // hello_world
        
        ArrayDelimiter, // ;
        ListDelimiter, // ,
        
        ArrayOpener, // [
        ArrayCloser, // ]
        
        CompoundOpener, // {
        CompoundCloser, // }
        Colon,
    }
    
}