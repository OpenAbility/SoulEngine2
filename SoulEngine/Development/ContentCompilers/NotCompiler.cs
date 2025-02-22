namespace SoulEngine.Development.ContentCompilers;

public class NotCompiler : ContentCompiler
{
    public override bool ShouldRecompile(ContentData contentData)
    {
        return File.GetLastWriteTime(contentData.InputFilePath) >= contentData.LastOutputWrite;
    }

    public override void Recompile(ContentData contentData)
    {
        if(File.Exists(contentData.OutputFilePath))
            File.Delete(contentData.OutputFilePath);
        
        using FileStream input = File.OpenRead(contentData.InputFilePath);
        using FileStream output = File.OpenWrite(contentData.OutputFilePath);

        BinaryWriter writer = new BinaryWriter(output);

        byte[] buffer = new byte[1024];
        int read = 0;

        while ((read = input.Read(buffer)) != 0)
        {
            for (int i = 0; i < read; i++)
            {
                writer.Write((byte)~buffer[i]);
            }
            writer.Flush();
        }

    }

    public override string GetCompiledPath(string path)
    {
        return path;
    }
}