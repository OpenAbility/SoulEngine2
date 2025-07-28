using SoulEngine.SequenceScript.Lexing;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes;

public class MetaStatement : SyntaxNode
{
    public Token Key;
    public Token Value;
}