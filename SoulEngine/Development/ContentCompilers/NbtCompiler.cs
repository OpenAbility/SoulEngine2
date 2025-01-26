using SoulEngine.Data;
using SoulEngine.Data.NBT;

namespace SoulEngine.Development.ContentCompilers;

public class NbtCompiler : ContentCompiler
{
    public string Extension;

    public NbtCompiler(string extension)
    {
        Extension = extension;
    }
    
    public override bool ShouldRecompile(ContentData contentData)
    {
        return File.GetLastWriteTime(contentData.InputFilePath) >= contentData.LastOutputWrite;
    }

    public override void Recompile(ContentData contentData)
    {
        using FileStream fileStream = File.OpenWrite(contentData.OutputFilePath);
        Tag tag = TagIO.ReadSNBT(File.ReadAllText(contentData.InputFilePath));
        TagIO.WriteCompressed(tag, fileStream);
    }

    public override string GetCompiledPath(string path)
    {
        return Path.ChangeExtension(path, Extension);
    }
}