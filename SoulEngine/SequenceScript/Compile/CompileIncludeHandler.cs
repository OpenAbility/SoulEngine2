namespace SoulEngine.SequenceScript.Compile;

public interface ICompileIncludeHandler
{
    public FileInfo FindFile(string from, string path);
}