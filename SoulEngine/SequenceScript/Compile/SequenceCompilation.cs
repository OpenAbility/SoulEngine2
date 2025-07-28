using System.IO.Enumeration;
using System.Security.Cryptography;
using System.Text;
using OpenAbility.Logging;
using SoulEngine.Data;

namespace SoulEngine.SequenceScript.Compile;

public static class SequenceCompilation
{
    private static readonly Logger Logger = Logger.Get("SequenceCompilation");
    
    public static void CompileDirectory(DirectoryCompileOptions compileOptions)
    {
        bool modifiedStandardLibrary = false;

        byte[] stdlibHash = SHA256.HashData(Encoding.UTF8.GetBytes(compileOptions.DefaultLibrarySource));
        
        if (!compileOptions.CompileRegistry.Exists("sequencescript/stdlib_hash"))
            modifiedStandardLibrary = true;
        else if (!Enumerable.SequenceEqual(compileOptions.CompileRegistry.GetBlob("sequencescript/stdlib_hash"),
                     stdlibHash))
            modifiedStandardLibrary = true;
        
        
        Dictionary<string, FileCompileInformation> files = new Dictionary<string, FileCompileInformation>();
        HashSet<string> recompileFiles = new HashSet<string>();

        CompilerContext context = new CompilerContext();
        
        context.CreateStandardLib(compileOptions.DefaultLibrarySource);

        if (!Directory.Exists(compileOptions.OutputDirectory))
            Directory.CreateDirectory(compileOptions.OutputDirectory);

        Logger.Info("Marking files as changed...");
        foreach (var file in Directory.GetFiles(compileOptions.InputDirectory, "*.*", SearchOption.AllDirectories))
        {
            string resolvePath = Path.GetRelativePath(compileOptions.InputDirectory, file);
            
            if(!FileSystemName.MatchesSimpleExpression(compileOptions.Pattern, resolvePath))
                continue;
            
            if(FileSystemName.MatchesSimpleExpression(compileOptions.ExcludePattern, resolvePath))
                continue;
            
            string output = Path.Join(compileOptions.OutputDirectory, resolvePath);
            
            if (!Directory.Exists(Path.GetDirectoryName(output)))
                Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            
            FileCompileInformation compileInformation = new FileCompileInformation();

            compileInformation.InputPath = file;
            compileInformation.OutputPath = output;
            compileInformation.ResolvePath = resolvePath;
            compileInformation.WasModified = File.GetLastWriteTimeUtc(file) >= File.GetLastWriteTimeUtc(output);

            if (compileInformation.WasModified)
            {
                recompileFiles.Add(resolvePath);
                compileInformation.Recompiled = true;

                if (compileOptions.CompileRegistry.Exists("sequencescript/files/" + resolvePath + "/deps_count"))
                {
                    int depCount =
                        compileOptions.CompileRegistry.GetInt32("sequencescript/files/" + resolvePath + "/deps_count");
                    for (int i = 0; i < depCount; i++)
                    {
                        string dep =
                            compileOptions.CompileRegistry.GetString("sequencescript/files/" + resolvePath + "/deps_" + i);
                        
                        recompileFiles.Add(dep);
                    }
                }
            }

            files[resolvePath] = compileInformation;
        }

        foreach (var file in files.Values)
        {
            if(file.Recompiled)
                continue;
            
            bool modifiedDependency = false;

            if (modifiedStandardLibrary)
                modifiedDependency = true;
            else if (!compileOptions.CompileRegistry.Exists("sequencescript/files/" + file.ResolvePath + "/deps_count"))
                modifiedDependency = true;
            else
            {
                int depCount = compileOptions.CompileRegistry.GetInt32("sequencescript/files/" + file.ResolvePath + "/deps_count");
                for (int i = 0; i < depCount; i++)
                {
                    string dep = compileOptions.CompileRegistry.GetString("sequencescript/files/" + file.ResolvePath + "/deps_" + i);

                    if (files[dep].Recompiled)
                    {
                        recompileFiles.Add(dep);
                        modifiedDependency = true;
                    }
                }
            }

            if (modifiedDependency)
            {
                file.Recompiled = true;
                recompileFiles.Add(file.ResolvePath);
            }
        }

        foreach (var file in recompileFiles)
        {
            if(!files.ContainsKey(file))
                continue;
            
            Logger.Debug("Compiling SequenceScript file {}", file);
            FileCompileInformation compileInfo = files[file];
            
            context.BeginCompiling(compileInfo.ResolvePath, compileInfo.InputPath, compileInfo.OutputPath);

            int depIndex = 0;
            foreach (var dep in context.GetFileDependencies(compileInfo.ResolvePath))
            {
                compileOptions.CompileRegistry.SetString("sequencescript/files/" + file + "/deps_" + depIndex, dep);
                depIndex++;
            }

            compileOptions.CompileRegistry.SetInt32("sequencescript/files/" + file + "/deps_count", depIndex);

        }

        compileOptions.CompileRegistry.SetBlob("sequencescript/stdlib_hash", stdlibHash);

        context.Throw();
        context.Emit();
        context.Throw();
        
        
        
    }
    
    private class FileCompileInformation
    {
        public string InputPath = "";
        public string OutputPath = "";
        public string ResolvePath = "";

        public bool WasModified;
        public bool Recompiled;
    }
}

public struct DirectoryCompileOptions()
{
    public string DefaultLibrarySource = null!;
    public string InputDirectory = null!;
    public string OutputDirectory = null!;

    public string Pattern = "*.ss";
    public string ExcludePattern = "";

    public DataRegistry CompileRegistry;
}