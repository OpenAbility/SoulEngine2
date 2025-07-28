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

    private void CompileDirectory(DirectoryInfo input, DirectoryInfo output, DirectoryInfo current)
    {
        foreach (var file in current.GetFiles())
        {
            string relative = Path.GetRelativePath(input.FullName, file.FullName);
            
            ContentCompiler? compiler = FindCompiler(relative);

            if (compiler == null)
                continue;

            FileInfo outputFileInfo = new FileInfo(Path.Combine(output.FullName, compiler.GetCompiledPath(relative)));

            outputFileInfo.Directory!.Create();

            DateTime lastOutput = outputFileInfo.LastWriteTimeUtc;

            ContentData contentData =
                new ContentData(input, output, file, outputFileInfo, lastOutput, game.DevelopmentRegistry);
            
            //Logger.Info("Testing file {}({} => {})", relative, inputFile, outputPath);

            if (!compiler.ShouldRecompile(contentData))
                continue;
            
            //Logger.Info("Compiling!");
            
            compiler.Recompile(contentData);
        }

        foreach (var subdirectory in current.GetDirectories())
        {
            CompileDirectory(input, output, subdirectory);
        }
        
    }
    
    private void PerformCompile(string input, string output)
    {
        
        lastCompileDate = DateTime.Now;

        if (!Directory.Exists(output))
            Directory.CreateDirectory(output);

        game.DevelopmentRegistry.SetString("_compilerVersion", EngineData.ContentCompiler.ToString());
        
        CompileDirectory(new DirectoryInfo(input), new DirectoryInfo(output), new DirectoryInfo(input));
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
    public readonly DirectoryInfo InputDirectory;
    /// <summary>
    /// The output asset directory
    /// </summary>
    public readonly DirectoryInfo OutputDirectory;

    /// <summary>
    /// The input file info
    /// </summary>
    public readonly FileInfo InputFile;
    /// <summary>
    /// The output file info
    /// </summary>
    public readonly FileInfo OutputFile;

    /// <summary>
    /// The last time the output file was written
    /// </summary>
    public readonly DateTime LastOutputWrite;

    /// <summary>
    /// A registry used to store extra data between steps or even compiles
    /// </summary>
    public readonly DataRegistry Registry;

    public ContentData(DirectoryInfo inputDirectory, DirectoryInfo outputDirectory, FileInfo inputFile, FileInfo outputFile, DateTime lastOutputWrite, DataRegistry registry)
    {
        InputDirectory = inputDirectory;
        OutputDirectory = outputDirectory;
        InputFile = inputFile;
        OutputFile = outputFile;
        LastOutputWrite = lastOutputWrite;
        Registry = registry;
    }


    public string FileDataPath(string id)
    {
        return "_contentCompile." + EngineData.ContentCompiler + "." + InputFile.FullName + "=" + OutputFile.FullName + "." + id;
    }
}