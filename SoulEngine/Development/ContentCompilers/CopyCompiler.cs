using SoulEngine.Data;

namespace SoulEngine.Development.ContentCompilers;

public class CopyCompiler : ContentCompiler
{
    public override bool ShouldRecompile(ContentData contentData)
    {
        return File.GetLastWriteTime(contentData.InputFilePath) >= contentData.LastOutputWrite;
    }

    public override void Recompile(ContentData contentData)
    {
        File.Copy(contentData.InputFilePath, contentData.OutputFilePath, true);
    }

    public override string GetCompiledPath(string path)
    {
        return path;
    }
}