using SoulEngine.SequenceScript.Lexing;

namespace SoulEngine.SequenceScript.Parsing.SyntaxNodes;

public class ImportNode : SyntaxNode
{
    public Token Target;
}