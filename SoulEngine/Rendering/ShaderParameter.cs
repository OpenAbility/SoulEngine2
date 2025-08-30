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
                ShaderParameterType.Sampler1dArray => true,
                ShaderParameterType.Sampler2dArray => true,
                ShaderParameterType.SamplerBuffer => true,
                ShaderParameterType.Sampler1dArrayShadow => true,
                ShaderParameterType.Sampler2dArrayShadow => true,
                ShaderParameterType.SamplerCubeShadow => true,
                ShaderParameterType.IntSampler1d => true,
                ShaderParameterType.IntSampler2d => true,
                ShaderParameterType.IntSampler3d => true,
                ShaderParameterType.IntSamplerCube => true,
                ShaderParameterType.IntSampler2dRect => true,
                ShaderParameterType.IntSampler1dArray => true,
                ShaderParameterType.IntSampler2dArray => true,
                ShaderParameterType.IntSamplerBuffer => true,
                ShaderParameterType.UnsignedIntSampler1d => true,
                ShaderParameterType.UnsignedIntSampler2d => true,
                ShaderParameterType.UnsignedIntSampler3d => true,
                ShaderParameterType.UnsignedIntSamplerCube => true,
                ShaderParameterType.UnsignedIntSampler2dRect => true,
                ShaderParameterType.UnsignedIntSampler1dArray => true,
                ShaderParameterType.UnsignedIntSampler2dArray => true,
                ShaderParameterType.UnsignedIntSamplerBuffer => true,
                ShaderParameterType.SamplerCubeMapArray => true,
                ShaderParameterType.SamplerCubeMapArrayShadow => true,
                ShaderParameterType.IntSamplerCubeMapArray => true,
                ShaderParameterType.UnsignedIntSamplerCubeMapArray => true,
                ShaderParameterType.Sampler2dMultisample => true,
                ShaderParameterType.IntSampler2dMultisample => true,
                ShaderParameterType.UnsignedIntSampler2dMultisample => true,
                ShaderParameterType.Sampler2dMultisampleArray => true,
                ShaderParameterType.IntSampler2dMultisampleArray => true,
                ShaderParameterType.UnsignedIntSampler2dMultisampleArray => true,
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
    Sampler1dArray = 36288,
    Sampler2dArray = 36289,
    SamplerBuffer = 36290,
    Sampler1dArrayShadow = 36291,
    Sampler2dArrayShadow = 36292,
    SamplerCubeShadow = 36293,
    IntSampler1d = 36297,
    IntSampler2d = 36298,
    IntSampler3d = 36299,
    IntSamplerCube = 36300,
    IntSampler2dRect = 36301,
    IntSampler1dArray = 36302,
    IntSampler2dArray = 36303,
    IntSamplerBuffer = 36304,
    UnsignedIntSampler1d = 36305,
    UnsignedIntSampler2d = 36306,
    UnsignedIntSampler3d = 36307,
    UnsignedIntSamplerCube = 36308,
    UnsignedIntSampler2dRect = 36309,
    UnsignedIntSampler1dArray = 36310,
    UnsignedIntSampler2dArray = 36311,
    UnsignedIntSamplerBuffer = 36312,
    SamplerCubeMapArray = 36876,
    SamplerCubeMapArrayShadow = 36877,
    IntSamplerCubeMapArray = 36878,
    UnsignedIntSamplerCubeMapArray = 36879,
    Sampler2dMultisample = 37128,
    IntSampler2dMultisample = 37129,
    UnsignedIntSampler2dMultisample = 37130,
    Sampler2dMultisampleArray = 37131,
    IntSampler2dMultisampleArray = 37132,
    UnsignedIntSampler2dMultisampleArray = 37133,
    
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