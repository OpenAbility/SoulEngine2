namespace SoulEngine.SequenceScript.Compile;

public struct CompileError(CodeLocation location, string errorCode, string message)
{
    public readonly CodeLocation Location = location;
    public readonly string ErrorCode = errorCode;
    public readonly string Message = message;

    public override string ToString()
    {
        return $"{Location}: error {ErrorCode}: {Message}";
    }
}