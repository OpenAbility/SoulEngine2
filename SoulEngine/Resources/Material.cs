using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;
using SoulEngine.Content;
using SoulEngine.Data;
using SoulEngine.Renderer;
using SoulEngine.Rendering;

namespace SoulEngine.Resources;

[Resource("e.mat", typeof(Loader))]
[ExpectedExtensions(".mat")]
public class Material : Resource
{
    

    public Shader Shader { get; private set; } = null!;
    public string Path { get; private set; } = null!;
    private Dictionary<string, object> values = new Dictionary<string, object>();

    private static Texture? mipTexture;
    private ResourceManager resourceManager = null!;

    private const int TextureBindingPoint = 3;

    public void BindShader() => Shader.Bind();
    
    public void BindCamera(CameraSettings cameraSettings, Matrix4 modelMatrix)
    {
        Shader.Matrix("um_projection", cameraSettings.ProjectionMatrix, false);
        Shader.Matrix("um_view", cameraSettings.ViewMatrix, false);
        Shader.Matrix("um_model", modelMatrix, false);
        Shader.Uniform3f("um_camera_direction", cameraSettings.CameraDirection);
    }

    public void BindShadowPassCamera(ShadowCameraSettings shadowCameraSettings, Matrix4 modelMatrix)
    {
        Shader.Matrix("um_projection", shadowCameraSettings.ProjectionMatrix, false);
        Shader.Matrix("um_view", shadowCameraSettings.ViewMatrix, false);
        Shader.Matrix("um_model", modelMatrix, false);
        Shader.Uniform3f("um_camera_direction", shadowCameraSettings.Direction);
    }

    public void BindShadows(ShadowCameraSettings shadowCameraSettings, Depthbuffer[] shadowBuffers)
    {
        Shader.Matrix("um_shadow_projection", shadowCameraSettings.ProjectionMatrix, false);
        Shader.Matrix("um_shadow_view", shadowCameraSettings.ViewMatrix, false);
        
        Shader.Uniform3f("um_shadow_direction", shadowCameraSettings.Direction);

        for (uint i = 0; i < shadowBuffers.Length; i++)
        {
            shadowBuffers[i].BindDepth(i);
        }
        
        Shader.Uniform1i("ut_shadow_buffers[0]", [0, 1, 2]);

    }

    public void BindUniforms()
    {
        uint textureBindingPoint = TextureBindingPoint;
        
        foreach (var value in values)
        {
            if (value.Value is Texture texture)
            {

                if (EngineVarContext.Global.GetBool("e_showmips", false))
                {
                    mipTexture ??= resourceManager.Load<Texture>("tex/mipmap_display.dds");
                    texture = mipTexture;
                }
                
                uint idx = textureBindingPoint++;
                texture.Bind(idx);
                Shader.Uniform1i(value.Key, (int)idx);
            } else if (value.Value is Vector4 vec4)
            {
                Shader.Uniform4f(value.Key, vec4);
            }
        }
    }

    private void Load(ResourceData data)
    {
        resourceManager = data.ResourceManager;
        
        Path = data.ResourcePath;
        MaterialDefinition materialDefinition = JsonConvert.DeserializeObject<MaterialDefinition>(data.ReadResourceString());

        if (materialDefinition.Shader == null)
            throw new Exception("No shader bound to material!");

        Shader = data.ResourceManager.Load<Shader>(materialDefinition.Shader);

        foreach (var parameter in Shader.Parameters)
        {
            if (materialDefinition.Values.TryGetValue(parameter.Name, out var jsonValue))
            {
                if (parameter.Type == ShaderParameterType.FloatVec2)
                    values[parameter.Name] = new Vector2(((JArray)jsonValue)[0].Value<float>(),
                        ((JArray)jsonValue)[1].Value<float>());
                if (parameter.Type == ShaderParameterType.FloatVec3)
                    values[parameter.Name] = new Vector3(((JArray)jsonValue)[0].Value<float>(),
                        ((JArray)jsonValue)[1].Value<float>(),
                        ((JArray)jsonValue)[2].Value<float>());
                if (parameter.Type == ShaderParameterType.FloatVec4)
                    values[parameter.Name] = new Vector4(((JArray)jsonValue)[0].Value<float>(),
                        ((JArray)jsonValue)[1].Value<float>(), ((JArray)jsonValue)[2].Value<float>(), ((JArray)jsonValue)[3].Value<float>());
                
                if (parameter.Type == ShaderParameterType.Float)
                    values[parameter.Name] = jsonValue.Value<float>();
                if (parameter.Type == ShaderParameterType.Bool)
                    values[parameter.Name] = jsonValue.Value<bool>();
                if (parameter.Type == ShaderParameterType.Int)
                    values[parameter.Name] = jsonValue.Value<int>();
                if (parameter.Type == ShaderParameterType.Double)
                    values[parameter.Name] = jsonValue.Value<double>();
                
                if (parameter.IsSampler)
                    values[parameter.Name] = data.ResourceManager.Load<Texture>(jsonValue.Value<string>()!);

            } else if (parameter.IsSampler)
            {
                values[parameter.Name] = data.ResourceManager.Load<Texture>("__TEXTURE_AUTOGEN/white");
            }
        }
    }
    
    internal struct MaterialDefinition
    {
        [JsonProperty("parent")] public string? Parent;
        [JsonProperty("shader")] public string? Shader;
        [JsonProperty("values")] public JObject Values;
    }
    
    public class Loader : IResourceLoader<Material>
    {
        public Material LoadResource(ResourceData data)
        {
            Material material = new Material();
            material.Load(data);
            return material;
        }
    }
}