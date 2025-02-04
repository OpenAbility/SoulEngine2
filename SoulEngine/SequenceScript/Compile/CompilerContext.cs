using System.Text;
using SoulEngine.SequenceScript.Lexing;
using SoulEngine.SequenceScript.Parsing;

namespace SoulEngine.SequenceScript.Compile;

/// <summary>
/// Context for the compilation of SequenceScript files
/// </summary>
public class CompilerContext
{
    private readonly List<ICompileIncludeHandler> includeHandlers = new List<ICompileIncludeHandler>();

    private readonly List<CompileError> errors = new List<CompileError>();

    public void Include(ICompileIncludeHandler includeHandler)
    {
        includeHandlers.Add(includeHandler);
    }

    public void Include(string directory)
    {
        Include(new DirectoryIncludeHandler(directory));
    }

    public void Compile(Stream inputStream, Stream outputStream, string path)
    {

        SequenceLexer lexer = new SequenceLexer(path, inputStream, Encoding.Default, this);
        lexer.Process();

        Token[] tokens = lexer.GetTokens();
        for (int i = 0; i < tokens.Length; i++)
        {
            //Console.WriteLine(tokens[i]);
        }

        SequenceParser parser = new SequenceParser(tokens, this);
        var ast = parser.Process();

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

        for (int i = 0; i < errors.Count; i++)
        {
            Console.Error.WriteLine(errors[i].ToString());
        }

        throw new Exception("Compilation threw errors!");
    }

    public void ResetErrors()
    {
        errors.Clear();
    }


}