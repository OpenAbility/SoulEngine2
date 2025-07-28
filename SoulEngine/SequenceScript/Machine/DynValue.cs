namespace SoulEngine.SequenceScript.Machine;

public struct DynValue
{

    public DynValue(float value) : this(ValueType.Floating, value)
    {
        
    }
    
    public DynValue(int value) : this(ValueType.Integer, value)
    {
        
    }
    
    public DynValue(bool value) : this(ValueType.Boolean, value)
    {
        
    }
    
    public DynValue(string value) : this(ValueType.String, value)
    {
        
    }
    
    public DynValue(ValueType type, object value)
    {
        Type = type;
        underlying = value;
    }
    
    public ValueType Type { get; private set; }

    private object underlying;

    public float Float
    {
        get => EnsureGet<float>(ValueType.Floating);
        set => EnsureSet(ValueType.Floating, value);
    }
    
    public int Int
    {
        get => EnsureGet<int>(ValueType.Integer);
        set => EnsureSet(ValueType.Integer, value);
    }
    
    public bool Bool
    {
        get => EnsureGet<bool>(ValueType.Boolean);
        set => EnsureSet(ValueType.Boolean, value);
    }
    
    public string String
    {
        get => EnsureGet<string>(ValueType.String);
        set => EnsureSet(ValueType.String, value);
    }
    
    public object Handle
    {
        get => EnsureGet<object>(ValueType.Handle);
        set => EnsureSet(ValueType.Handle, value);
    }

    public object Raw => underlying;

    private T EnsureGet<T>(ValueType type)
    {
        if (Type != type)
            throw new Exception("DynValue is not of type " + type);
        return (T)underlying;
    }
    
    private void EnsureSet<T>(ValueType type, T value)
    {
        if (Type != type)
            throw new Exception("DynValue is not of type " + type);
        underlying = value!;
    }

    public float AsFloat
    {
        get
        {
            if (Type == ValueType.Integer)
                return (int)underlying;
            else if (Type == ValueType.Floating)
                return (float)underlying;
            throw new Exception("Cannot interpret DynValue as float!");
        }
    }
}