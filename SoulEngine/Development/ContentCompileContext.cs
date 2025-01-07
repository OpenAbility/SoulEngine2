using System.IO.Enumeration;
using OpenAbility.Logging;
using SoulEngine.Core;

namespace SoulEngine.Development;

public class ContentCompileContext
{
    private readonly Game game;

    private readonly Logger Logger;

    private readonly List<Timer> timers = new List<Timer>();

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
    
    private DateTime lastCompileDate = DateTime.UnixEpoch;

    public void CompileDirectory(string input, string output)
    {

        var spyDirectory = new DirectoryInfo(input);
        PerformCompile(input, output);

        Timer refreshTimer = new Timer(o =>
        {
            DateTime lastModifyDate = spyDirectory.EnumerateFiles("*", SearchOption.AllDirectories)
                .OrderByDescending(f => f.LastWriteTime).Select(f => f.LastWriteTime).FirstOrDefault(DateTime.UnixEpoch);
            
            if (lastModifyDate >= lastCompileDate)
            {
                PerformCompile(input, output);
                game.ResourceManager.ReloadAll();
            }

        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.5f));
        timers.Add(refreshTimer);

    }
    
    private void PerformCompile(string input, string output)
    {
        
        lastCompileDate = DateTime.Now;

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

            string outputPathDir = Path.GetDirectoryName(outputPath)!;
            if (!Directory.Exists(outputPathDir))
                Directory.CreateDirectory(outputPathDir);
            
            DateTime lastOutput = File.GetLastWriteTime(outputPath);
            
            Logger.Info("Testing file {}({} => {})", relative, inputFile, outputPath);

            if (!compiler.ShouldRecompile(lastOutput, inputFile, game.DevelopmentRegistry))
                continue;
            
            Logger.Info("Compiling!");
            
            compiler.Recompile(inputFile, outputPath, game.DevelopmentRegistry);
        }
        
    }
}