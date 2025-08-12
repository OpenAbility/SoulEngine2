using OpenTK.Mathematics;
using SoulEngine.Components.Lighting;
using SoulEngine.Rendering;

namespace SoulEngine.Renderer;

public struct LightSubmitInformation
{
    public Mesh LightMesh;
    public Matrix4 ModelMatrix;
    public LightComponent Component;

    public Vector3 Direction;
    
    public Shader LightShader;

    public LightShaderBind? ShaderBind;
    public object? UserData;

    public LightSubmitInformation(Mesh mesh, Matrix4 modelMatrix, LightComponent component, Shader lightShader)
    {
        LightMesh = mesh;
        ModelMatrix = modelMatrix;
        Component = component;
        LightShader = lightShader;
    }
}

public delegate void LightShaderBind(in LightSubmitInformation submitInfo);