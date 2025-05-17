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
        return File.GetLastWriteTime(contentData.InputFile.FullName) >= contentData.LastOutputWrite;
    }

    public override void Recompile(ContentData contentData)
    {
        using FileStream fileStream = File.OpenWrite(contentData.OutputFile.FullName);
        Tag tag = TagIO.ReadSNBT(File.ReadAllText(contentData.InputFile.FullName));
        TagIO.WriteCompressed(tag, fileStream);
    }

    public override string GetCompiledPath(string path)
    {
        return Path.ChangeExtension(path, Extension);
    }
}