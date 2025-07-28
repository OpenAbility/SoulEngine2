namespace SoulEngine.SequenceScript.Machine;

public struct Instruction
{
    public readonly OpCode OpCode;
    public readonly DynValue Parameter;

    public Instruction(OpCode opCode, DynValue parameter)
    {
        OpCode = opCode;
        Parameter = parameter;
    }
}