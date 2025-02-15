namespace SoulEngine.SequenceScript.Machine;

public enum ValueType
{
    /// <summary>
    /// 32-bit signed integer
    /// </summary>
    Integer,
    /// <summary>
    /// 32-bit float
    /// </summary>
    Floating,
    /// <summary>
    /// 8-bit boolean
    /// </summary>
    Boolean,
    /// <summary>
    /// It's a string
    /// </summary>
    String,
    /// <summary>
    /// Opaque device handle
    /// </summary>
    Handle
}