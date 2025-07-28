using System.Diagnostics;

namespace SoulEngine.Development.ContentCompilers;

public class DDSCompiler : ContentCompiler
{
    public override bool ShouldRecompile(ContentData contentData)
    {
        return contentData.InputFile.LastWriteTimeUtc >= contentData.LastOutputWrite;
    }

    public override void Recompile(ContentData contentData)
    {
        string ddsCompiler =
            Environment.GetEnvironmentVariable("E_DDS_COMPILER") ?? "bash";
        string ddsArgs =
            Environment.GetEnvironmentVariable("E_DDS_ARGS") ?? "./content_src/compile_dds.sh \"%input%\" dxt5 \"%output%\"";

        ddsArgs = ddsArgs.Replace("%input%", contentData.InputFile.FullName);
        ddsArgs = ddsArgs.Replace("%output%", contentData.OutputFile.FullName);

        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = ddsCompiler;
        start.Arguments = ddsArgs;
        
        Process? process = Process.Start(start);
        process?.WaitForExit();

        if (process == null)
            throw new Exception("Could not invoke DDS compiler!");
    }

    public override string GetCompiledPath(string path)
    {
        return Path.ChangeExtension(path, ".dds");
    }
}