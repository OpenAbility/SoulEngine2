using System.Text;
using SoulEngine.SequenceScript.Emitter;
using SoulEngine.SequenceScript.Lexing;
using SoulEngine.SequenceScript.Parsing;
using SoulEngine.SequenceScript.Parsing.SyntaxNodes;
using SoulEngine.SequenceScript.Utility;

namespace SoulEngine.SequenceScript.Compile;

/// <summary>
/// Context for the compilation of SequenceScript files
/// </summary>
public class CompilerContext
{
    private readonly List<CompileError> errors = new List<CompileError>();

    private readonly Dictionary<string, CompilingFile> working = new Dictionary<string, CompilingFile>();
    
    private CompilingFile standardLibrary = null!;
    public void CreateStandardLib(string code)
    {
        standardLibrary = new CompilingFile();
        standardLibrary.InputPath = "__STDLIB";
        standardLibrary.OutputPath = "__STDLIB";
        standardLibrary.ResolvePath = "__STDLIB";

        working["__STDLIB"] = standardLibrary;
        
        var lexer = new SequenceLexer(standardLibrary.InputPath, new MemoryStream(Encoding.UTF8.GetBytes(code)), Encoding.UTF8, this);
        lexer.Process();
        
        standardLibrary.Tokens = lexer.GetTokens();

        var parser = new SequenceParser(standardLibrary.Tokens, this);

        standardLibrary.AST = parser.Process();

        foreach (var node in standardLibrary.AST.Nodes)
        {
            if (node is GlobalStatement globalStatement)
            {
                standardLibrary.Globals.Add(globalStatement.Identifier.Value, SequenceRules.KeywordToValueType(globalStatement.Type.TokenType));
            } 
            else if (node is ProcedureDefinitionNode procedureDefinitionNode)
            {
                CompilingFunction function = new CompilingFunction();

                function.ReturnType = SequenceRules.KeywordToReturnType(procedureDefinitionNode.ReturnType.TokenType);
                function.Name = procedureDefinitionNode.Identifier.Value;
                function.ParameterTypes = procedureDefinitionNode.Parameters
                    .Select(n => SequenceRules.KeywordToValueType(n.Type.TokenType)).ToArray();
                function.SystemFunction = procedureDefinitionNode.Extern;
                
                standardLibrary.Functions[function.Name] = function;
            }
        }
    }

    public void BeginCompiling(string resolvePath, string inputPath, string outputPath)
    {
        
        CompilingFile compilingFile = new CompilingFile();
        working[resolvePath] = compilingFile;

        compilingFile.InputPath = inputPath;
        compilingFile.OutputPath = outputPath;
        compilingFile.ResolvePath = resolvePath;

        using var inputStream = File.OpenRead(inputPath);

        var lexer = new SequenceLexer(inputPath, inputStream, Encoding.UTF8, this);
        lexer.Process();
        
        compilingFile.Tokens = lexer.GetTokens();

        var parser = new SequenceParser(compilingFile.Tokens, this);

        compilingFile.AST = parser.Process();

        foreach (var node in compilingFile.AST.Nodes)
        {
            if (node is GlobalStatement globalStatement)
            {
                compilingFile.Globals.Add(globalStatement.Identifier.Value, SequenceRules.KeywordToValueType(globalStatement.Type.TokenType));
            } 
            else if (node is ProcedureDefinitionNode procedureDefinitionNode)
            {
                CompilingFunction function = new CompilingFunction();

                function.ReturnType = SequenceRules.KeywordToReturnType(procedureDefinitionNode.ReturnType.TokenType);
                function.Name = procedureDefinitionNode.Identifier.Value;
                function.ParameterTypes = procedureDefinitionNode.Parameters
                    .Select(n => SequenceRules.KeywordToValueType(n.Type.TokenType)).ToArray();

                compilingFile.Functions[function.Name] = function;
            }
        }
    }

    public IEnumerable<string> GetFileDependencies(string resolvePath)
    {
        CompilingFile compilingFile = working[resolvePath];

        foreach (var node in compilingFile.AST.Nodes)
        {
            if (node is ImportNode importNode)
                yield return importNode.Target.Value;
        }

    }

    public CompilingFile? ResolveInclude(string from, string to)
    {
        string path = to == "__STDLIB" ? "__STDLIB" : Path.Join(Path.GetDirectoryName(from), to);
        return working.GetValueOrDefault(path);
    }

    public void Emit()
    {
        foreach (var compilingFile in working.Values)
        {
            if(compilingFile == standardLibrary)
                continue;
            
            SequenceEmitter emitter = new SequenceEmitter(this, compilingFile);
            emitter.Process(compilingFile.AST);
        }
    }

    public void Error(CompileError error)
    {
        errors.Add(error);
    }

    public void Error(CodeLocation location, string errorCode, string message)
    {
        Error(new CompileError(location, errorCode, message));
    }

    public void Throw()
    {
        if(errors.Count == 0)
            return;

        Console.ForegroundColor = ConsoleColor.Red;

        for (int i = 0; i < errors.Count; i++)
        {
            Console.Error.WriteLine(errors[i].ToString());
        }
        
        Console.ResetColor();

        throw new Exception("Compilation threw errors!");
    }

    public void ResetErrors()
    {
        errors.Clear();
    }
}