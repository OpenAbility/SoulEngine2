using System.IO.Enumeration;
using OpenAbility.Logging;
using SoulEngine.Data;

namespace SoulEngine.SequenceScript.Compile;

public static class SequenceCompilation
{
    private static readonly Logger Logger = Logger.Get("SequenceCompilation");
    
    public static void CompileDirectory(DirectoryCompileOptions compileOptions)
    {
        CompilerContext context = new CompilerContext();
        
        context.CreateStandardLib(compileOptions.DefaultLibrarySource);

        if (!Directory.Exists(compileOptions.OutputDirectory))
            Directory.CreateDirectory(compileOptions.OutputDirectory);

        foreach (var file in Directory.GetFiles(compileOptions.InputDirectory, "*.*", SearchOption.AllDirectories))
        {
            string resolvePath = Path.GetRelativePath(compileOptions.InputDirectory, file);
            
            if(!FileSystemName.MatchesSimpleExpression(compileOptions.Pattern, resolvePath))
                continue;
            
            if(FileSystemName.MatchesSimpleExpression(compileOptions.ExcludePattern, resolvePath))
                continue;
            
            string output = Path.Join(compileOptions.OutputDirectory, resolvePath);

            Logger.Debug("Compiling SequenceScript file {}", file);
            
            if (!Directory.Exists(Path.GetDirectoryName(output)))
                Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            
            context.BeginCompiling(resolvePath, file, output);
            
        }

        context.Throw();
        context.Emit();
        context.Throw();
        
    }
}

public struct DirectoryCompileOptions()
{
    public string DefaultLibrarySource;
    public string InputDirectory;
    public string OutputDirectory;

    public string Pattern = "*.ss";
    public string ExcludePattern = "";
}