namespace SoulEngine.SequenceScript.Compile;

public struct CodeLocation(string file, int line, int column)
{
    public int Line = line;
    public int Column = column;
    public string File = file;

    public override string ToString()
    {
        return $"{File} ({Line},{Column})";
    }
}