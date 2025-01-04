using SoulEngine.Data;

namespace SoulEngine.Development;

/// <summary>
/// Handles content compilation
/// </summary>
public abstract class ContentCompiler
{
    public abstract bool ShouldRecompile(DateTime lastOutput, string path, DataRegistry registry);
    
    public abstract void Recompile(string path, string output, DataRegistry registry);

    public abstract string GetCompiledPath(string path);
}