namespace SoulEngine.SequenceScript.Machine;

public class BinaryModule
{
    private readonly Dictionary<int, BinaryModule> resolveMapping = new Dictionary<int, BinaryModule>();
    private readonly Dictionary<string, DynValue> globals = new Dictionary<string, DynValue>();
    private readonly Dictionary<string, Instruction[]> procedures = new Dictionary<string, Instruction[]>();
    private readonly Dictionary<string, string> meta = new Dictionary<string, string>();
    
    private Instruction[] lastProcedure = null!;
    private int lastProcedureName;
    
    public BinaryModule(ExecutionContext executionContext)
    {
        
    }

    public void RegisterModuleMapping(int index, BinaryModule binaryModule)
    {
        resolveMapping[index] = binaryModule;
    }

    public void RegisterGlobal(string name, ValueType valueType)
    {
        globals[name] = valueType switch
        {
            ValueType.Integer => new DynValue(0),
            ValueType.Floating => new DynValue(0.0f),
            ValueType.Boolean => new DynValue(false),
            ValueType.String => new DynValue(""),
            ValueType.Handle => new DynValue(ValueType.Handle, null!),
            _ => throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null)
        };
    }

    public void SetMeta(string name, string value)
    {
        meta[name] = value;
    }
    
    public string? GetMeta(string name)
    {
        return meta.GetValueOrDefault(name);
    }
    
    public string GetMeta(string name, string defaultValue)
    {
        return GetMeta(name) ?? defaultValue;
    }

    public DynValue GetGlobal(string name)
    {
        return globals[name];
    }
    
    public void SetGlobal(string name, DynValue value)
    {
        if(globals.ContainsKey(name))
            globals[name] = value;
    }

    public Instruction GetProcedureInstruction(string name, int index)
    {
        if (name.GetHashCode() == lastProcedureName)
            return lastProcedure[index];
        
        lastProcedureName = name.GetHashCode();
        lastProcedure = procedures[name];
        
        return lastProcedure[index];
    }

    public void RegisterProcedure(string name, Instruction[] instructions)
    {
        procedures[name] = instructions;
    }

    public BinaryModule GetResolve(int moduleIndex)
    {
        return resolveMapping[moduleIndex];
    }
}