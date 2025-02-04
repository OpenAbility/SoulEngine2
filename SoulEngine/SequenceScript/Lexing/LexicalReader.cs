using System.Text;
using SoulEngine.SequenceScript.Compile;

namespace SoulEngine.SequenceScript.Lexing;

public class LexicalReader
{
    private readonly CompilerContext context;
    
    // TODO: This might be a bad idea - rework later maybe?
    private readonly string buffer;

    private CodeLocation location;
    
    private int currentIndex = 0;

    
    public LexicalReader(Stream stream, Encoding encoding, string filePath, CompilerContext context)
    {
        using MemoryStream memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        buffer = encoding.GetString(memoryStream.GetBuffer());
        
        this.context = context;

        location = new CodeLocation(filePath, 1, 0);
    }

    public char Peek(int amount)
    {
        if (currentIndex + amount >= buffer.Length)
            return '\0';
        return buffer[currentIndex + amount];
    }

    public char Current => Peek(0);

    public CodeLocation Location => location;

    public bool EOF => currentIndex >= buffer.Length;

    public void Step(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            if (Current == '\n')
            {
                location.Line++;
                location.Column = 0;
            } else if (Current == '\t')
            {
                location.Column += 4;
            }
            else
            {
                location.Column++;
            }

            currentIndex++;
        }
    }
    

}