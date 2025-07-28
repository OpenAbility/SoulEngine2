namespace SoulEngine.SequenceScript.Machine;

public enum OpCode : byte
{
    /// <summary>
    /// Push a const integer to stack
    /// </summary>
    PUSHI,
    /// <summary>
    /// Push a const float to stack
    /// </summary>
    PUSHF,
    /// <summary>
    /// Push a const bool to stack
    /// </summary>
    PUSHB,
    /// <summary>
    /// Push a const string to stack
    /// </summary>
    PUSHS,
    /// <summary>
    /// Push a null handle to stack
    /// </summary>
    PUSHN,
    
    /// <summary>
    /// Pop a value from stack
    /// </summary>
    POP,
    
    /// <summary>
    /// Clones the top value from stack
    /// </summary>
    CLONE,
    
    /// <summary>
    /// Store a value from stack into a variable
    /// </summary>
    STORE,
    /// <summary>
    /// Load a variable onto the stack
    /// </summary>
    LOAD,
    
    // Expected math instructions
    ADD,
    SUB,
    MULT,
    DIV,
    MOD,
    
    XOR,
    AND,
    OR,
    NOT,
    
    /// <summary>
    /// Compares top two values from stack, pushes true if greater than, otherwise pushes false
    /// </summary>
    IGT,
    /// <summary>
    /// Compares top two values from stack, pushes true if less than, otherwise pushes false
    /// </summary>
    ILT,
    /// <summary>
    /// Compares top two values from stack, pushes true if equal, otherwise pushes false
    /// </summary>
    IEQ,
    
    /// <summary>
    /// Jumps if top value of stack is true
    /// </summary>
    JMP,
    
    /// <summary>
    /// Calls a procedure
    /// </summary>
    CALL,
    
    /// <summary>
    /// Returns from a procedure
    /// </summary>
    RET,
    
    /// <summary>
    /// Calls a host "interrupt" - used to interface outside the sandbox
    /// </summary>
    INT
}