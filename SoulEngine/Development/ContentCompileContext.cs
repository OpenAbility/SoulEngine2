using System.IO.Enumeration;
using OpenAbility.Logging;
using SoulEngine.Core;

namespace SoulEngine.Development;

public class ContentCompileContext
{
    private readonly Game game;

    private readonly Logger Logger;

    private List<(string pattern, ContentCompiler compiler)> compilers = new ();
    
    public ContentCompileContext(Game game)
    {
        this.game = game;
        Logger = Logger.Get("Game", "ContentCompileContext");
    }

    public ContentCompileContext Register(string pattern, ContentCompiler compiler)
    {
        compilers.Add((pattern, compiler));
        return this;
    }

    private ContentCompiler? FindCompiler(string path)
    {
        foreach (var compiler in compilers)
        {
            if (FileSystemName.MatchesSimpleExpression(compiler.pattern, path))
                return compiler.compiler;
        }
        
        return null;
    }
    
    public void CompileDirectory(string input, string output)
    {

        if (!Directory.Exists(output))
            Directory.CreateDirectory(output);

        game.DevelopmentRegistry.SetString("_compilerVersion", EngineData.ContentCompiler.ToString());

        var sourceFiles = Directory.GetFiles(input, "*.*", SearchOption.AllDirectories);

        foreach (var inputFile in sourceFiles)
        {

            string relative = Path.GetRelativePath(input, inputFile);
            
            ContentCompiler? compiler = FindCompiler(relative);

            if (compiler == null)
                continue;
            
            string outputPath = Path.Combine(output, compiler.GetCompiledPath(relative));
            DateTime lastOutput = File.GetLastWriteTimeUtc(outputPath);
            
            Logger.Info("Testing file {}({} => {})", relative, inputFile, outputPath);

            if (!compiler.ShouldRecompile(lastOutput, inputFile, game.DevelopmentRegistry))
                continue;
            
            Logger.Info("Compiling!");
            
            compiler.Recompile(inputFile, outputPath, game.DevelopmentRegistry);
        }
        
    }
}