using SoulEngine.Data;

namespace SoulEngine.Development;

/// <summary>
/// Handles content compilation
/// </summary>
public abstract class ContentCompiler
{
    public abstract bool ShouldRecompile(ContentData contentData);
    
    public abstract void Recompile(ContentData contentData);

    public abstract string GetCompiledPath(string path);
}