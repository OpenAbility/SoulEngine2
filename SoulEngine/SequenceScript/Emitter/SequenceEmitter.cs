using SoulEngine.SequenceScript.Parsing.SyntaxNodes;

namespace SoulEngine.SequenceScript.Emitter;

/// <summary>
/// Handles binding and emitting of SequenceScript files
/// </summary>
public class SequenceEmitter
{

    private readonly List<string> stringTable = new List<string>();
    
    public SequenceEmitter()
    {
        
    }

    public void Process(ProgramRootNode rootNode)
    {
    }
}