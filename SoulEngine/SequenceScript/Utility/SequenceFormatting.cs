using SoulEngine.SequenceScript.Compile;

namespace SoulEngine.SequenceScript.Utility;

public static class SequenceFormatting
{
    public static string FormatCharacter(char c)
    {
        if (c == '\n')
            return "[newline]";
        if (c == '\r')
            return "[carriage return]";
        if (c == '\t')
            return "[tab]";
        if(char.IsControl(c))
            return "CONTROL CHAR";
        
        return c.ToString();
    }
    
    public static bool IsValidIdentifier(char c, bool start)
    {
        if (Char.IsAsciiLetter(c)) return true;
        if (c == '_') return true;
        if (!start && Char.IsAsciiDigit(c)) return true;
        return false;
    }

    public static char GetEscaped(CodeLocation location, char c, CompilerContext context)
    {
        if (c == 'n')
            return '\n';
        if (c == '\\')
            return '\\';

        context.Error(location, "SS1002", "Unrecognized escape code \\'" + FormatCharacter(c) + "'");
        return c;
    }
}