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
    
    public override bool ShouldRecompile(DateTime lastOutput, string path, DataRegistry registry)
    {
        return File.GetLastWriteTime(path) >= lastOutput;
    }

    public override void Recompile(string path, string output, DataRegistry registry)
    {
        using FileStream fileStream = File.OpenWrite(output);
        Tag tag = TagIO.ReadSNBT(File.ReadAllText(path));
        TagIO.WriteCompressed(tag, fileStream);
    }

    public override string GetCompiledPath(string path)
    {
        return Path.ChangeExtension(path, Extension);
    }
}