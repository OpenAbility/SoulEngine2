using SoulEngine.Data;

namespace SoulEngine.Development.ContentCompilers;

public class CopyCompiler : ContentCompiler
{
    public override bool ShouldRecompile(DateTime lastOutput, string path, DataRegistry registry)
    {
        return File.GetLastWriteTime(path) >= lastOutput;
    }

    public override void Recompile(string path, string output, DataRegistry registry)
    {
        File.WriteAllBytes(output, File.ReadAllBytes(path));
    }

    public override string GetCompiledPath(string path)
    {
        return path;
    }
}