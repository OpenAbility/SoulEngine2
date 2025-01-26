using System.IO.Enumeration;
using OpenAbility.Logging;
using SoulEngine.Core;
using SoulEngine.Data;

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

            ContentData contentData =
                new ContentData(input, output, inputFile, outputPath, lastOutput, game.DevelopmentRegistry);
            
            //Logger.Info("Testing file {}({} => {})", relative, inputFile, outputPath);

            if (!compiler.ShouldRecompile(contentData))
                continue;
            
            //Logger.Info("Compiling!");
            
            compiler.Recompile(contentData);
        }
        
    }
}

/// <summary>
/// Specifies all data needed to recompile resources
/// </summary>
public struct ContentData
{
    /// <summary>
    /// The input asset directory
    /// </summary>
    public readonly string InputDirectory;
    /// <summary>
    /// The output asset directory
    /// </summary>
    public readonly string OutputDirectory;

    /// <summary>
    /// The input file path
    /// </summary>
    public readonly string InputFilePath;
    /// <summary>
    /// The output file path
    /// </summary>
    public readonly string OutputFilePath;

    /// <summary>
    /// The last time the output file was written
    /// </summary>
    public readonly DateTime LastOutputWrite;

    /// <summary>
    /// A registry used to store extra data between steps or even compiles
    /// </summary>
    public readonly DataRegistry Registry;

    public ContentData(string inputDirectory, string outputDirectory, string inputFilePath, string outputFilePath, DateTime lastOutputWrite, DataRegistry registry)
    {
        InputDirectory = inputDirectory;
        OutputDirectory = outputDirectory;
        InputFilePath = inputFilePath;
        OutputFilePath = outputFilePath;
        LastOutputWrite = lastOutputWrite;
        Registry = registry;
    }


    public string FileDataPath(string id)
    {
        return "_contentCompile." + EngineData.ContentCompiler + "." + OutputFilePath + "=" + InputFilePath + "." + id;
    }
}