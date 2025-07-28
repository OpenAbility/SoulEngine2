using SoulEngine.Data;

namespace SoulEngine.Development.ContentCompilers;

public class CopyCompiler : ContentCompiler
{
    public override bool ShouldRecompile(ContentData contentData)
    {
        return contentData.InputFile.LastWriteTimeUtc >= contentData.LastOutputWrite;
    }

    public override void Recompile(ContentData contentData)
    {
        contentData.InputFile.CopyTo(contentData.OutputFile.FullName, true);
    }

    public override string GetCompiledPath(string path)
    {
        return path;
    }
}