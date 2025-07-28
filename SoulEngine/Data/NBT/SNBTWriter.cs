namespace SoulEngine.Data.NBT;

public class SNBTWriter : IDisposable
{
    private readonly TextWriter writer;
    private readonly bool leaveOpen;
    
    public SNBTWriter(TextWriter writer, bool leaveOpen)
    {
        this.writer = writer;
        this.leaveOpen = leaveOpen;
    }

    private int indent;
    private string indentStr = "";

    public SNBTWriter Indent()
    {
        indent++;
        indentStr = new string('\t', indent);
        return this;
    }
    
    public SNBTWriter PopIndent()
    {
        indent--;
        indentStr = new string('\t', indent);
        return this;
    }

    public SNBTWriter BeginLine(string s)
    {
        writer.Write(indentStr + s);
        return this;
    }
    
    public SNBTWriter Append(string s)
    {
        writer.Write(s);
        return this;
    }
    
    public SNBTWriter EndLine(string s)
    {
        writer.Write(s + "\n");
        return this;
    }
    
    

    public void Dispose()
    {
        if(!leaveOpen)
            writer.Close();
    }
}