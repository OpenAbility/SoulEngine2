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
    private readonly Dictionary<string, object> values = new Dictionary<string, object>();

    private static Texture? mipTexture;
    private ResourceManager resourceManager = null!;

    private const int TextureBindingPoint = 5;

    public void BindShader() => Shader.Bind();
    
    public void BindCamera(ShaderBinder binder, CameraSettings cameraSettings, Matrix4 modelMatrix)
    {
        binder.BindUniform("um_projection", cameraSettings.ProjectionMatrix, false);
        binder.BindUniform("um_view", cameraSettings.ViewMatrix, false);
        binder.BindUniform("um_model", modelMatrix, false);
        binder.BindUniform("um_camera_direction", cameraSettings.CameraDirection);
    }

    public void BindShadowPassCamera(ShaderBinder binder, Matrix4 view, Matrix4 projection, Matrix4 modelMatrix)
    {
        binder.BindUniform("um_projection", projection, false);
        binder.BindUniform("um_view", view, false);
        binder.BindUniform("um_model", modelMatrix, false);
    }
    
    public void BindUniforms(ShaderBinder binder)
    {
        uint textureBindingPoint = TextureBindingPoint;

        bool showMips = EngineVarContext.Global.GetBool("e_showmips", false);
        
        if (showMips)
            mipTexture ??= resourceManager.Load<Texture>("tex/mipmap_display.dds");

        Texture nullTexture = resourceManager.Load<Texture>("__TEXTURE_AUTOGEN/null");
        foreach (var parameter in Shader.Parameters)
        {

            if (!values.TryGetValue(parameter.Name, out var value))
            {
                if (parameter.IsSampler)
                {
                    binder.BindTexture(parameter.Name, nullTexture);
                    binder.BindUniform(parameter.Name + "_assigned", 0);
                }
             
                continue;
            }

            if (parameter.IsSampler)
            {
                binder.BindTexture(parameter.Name, showMips ? mipTexture! : (Texture)value);
                binder.BindUniform(parameter.Name + "_assigned", 1);
            } 
            else if (parameter.Type == ShaderParameterType.Float && value is float fvec1)
                binder.BindUniform(parameter.Name, fvec1);
            else if (parameter.Type == ShaderParameterType.FloatVec2 && value is Vector2 fvec2)
                binder.BindUniform(parameter.Name, fvec2);
            else if (parameter.Type == ShaderParameterType.FloatVec3 && value is Vector3 fvec3)
                binder.BindUniform(parameter.Name, fvec3);
            else if (parameter.Type == ShaderParameterType.FloatVec4 && value is Vector4 fvec4)
                binder.BindUniform(parameter.Name, fvec4);
            else if (parameter.Type == ShaderParameterType.FloatVec4 && value is Colour fcolour4)
                binder.BindUniform(parameter.Name, fcolour4);
            
            else if (parameter.Type == ShaderParameterType.Int && value is int ivec1)
                binder.BindUniform(parameter.Name, ivec1);
            else if (parameter.Type == ShaderParameterType.IntVec2 && value is Vector2i ivec2)
                binder.BindUniform(parameter.Name, ivec2);
            else if (parameter.Type == ShaderParameterType.IntVec3 && value is Vector3i ivec3)
                binder.BindUniform(parameter.Name, ivec3);
            else if (parameter.Type == ShaderParameterType.IntVec4 && value is Vector4i ivec4)
                binder.BindUniform(parameter.Name, ivec4);

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