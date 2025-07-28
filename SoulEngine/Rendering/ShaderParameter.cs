namespace SoulEngine.Rendering;

public struct ShaderParameter
{
    public readonly string Name;
    public readonly int Location;
    public readonly ShaderParameterType Type;
    public readonly int Count;
    
    public ShaderParameter(string name, int location, int count, ShaderParameterType type)
    {
        Name = name;
        Location = location;
        Type = type;
        Count = count;
    }

    public bool IsSampler
    {
        get
        {
            return Type switch
            {
                ShaderParameterType.Sampler1d => true,
                ShaderParameterType.Sampler2d => true,
                ShaderParameterType.Sampler3d => true,
                ShaderParameterType.SamplerCube => true,
                ShaderParameterType.Sampler1dShadow => true,
                ShaderParameterType.Sampler2dShadow => true,
                ShaderParameterType.Sampler2dRect => true,
                ShaderParameterType.Sampler2dRectShadow => true,
                _ => false
            };
        }
    }
}

public enum ShaderParameterType
{
    Int = 5124,
    UnsignedInt = 5125,
    Float = 5126,
    Double = 5130,
    FloatVec2 = 35664,
    FloatVec3 = 35665,
    FloatVec4 = 35666,
    IntVec2 = 35667,
    IntVec3 = 35668,
    IntVec4 = 35669,
    Bool = 35670,
    BoolVec2 = 35671,
    BoolVec3 = 35672,
    BoolVec4 = 35673,
    FloatMat2 = 35674,
    FloatMat3 = 35675,
    FloatMat4 = 35676,
    Sampler1d = 35677,
    Sampler2d = 35678,
    Sampler3d = 35679,
    SamplerCube = 35680,
    Sampler1dShadow = 35681,
    Sampler2dShadow = 35682,
    Sampler2dRect = 35683,
    Sampler2dRectShadow = 35684,
    FloatMat2x3 = 35685,
    FloatMat2x4 = 35686,
    FloatMat3x2 = 35687,
    FloatMat3x4 = 35688,
    FloatMat4x2 = 35689,
    FloatMat4x3 = 35690,
    DoubleMat2 = 36678,
    DoubleMat3 = 36679,
    DoubleMat4 = 36680,
    DoubleMat2x3 = 36681,
    DoubleMat2x4 = 36682,
    DoubleMat3x2 = 36683,
    DoubleMat3x4 = 36684,
    DoubleMat4x2 = 36685,
    DoubleMat4x3 = 36686,
    DoubleVec2 = 36860,
    DoubleVec3 = 36861,
    DoubleVec4 = 36862,
}