using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;
using SoulEngine.Content;
using SoulEngine.Data;
using SoulEngine.Rendering;

namespace SoulEngine.Resources;

[Resource(typeof(Loader))]
[ExpectedExtensions(".mat")]
public class Material : Resource
{
    

    public Shader Shader { get; private set; }
    public string Path { get; private set; }
    private Dictionary<string, object> values = new Dictionary<string, object>();

    private static Texture? mipTexture;
    private ResourceManager ResourceManager;
    
    
    public void Bind(CameraSettings cameraSettings, Matrix4 model)
    {
        Shader.Bind();
        Shader.Matrix("um_projection", cameraSettings.ProjectionMatrix, false);
        Shader.Matrix("um_view", cameraSettings.ViewMatrix, false);
        Shader.Matrix("um_model", model, false);
        Shader.Uniform3f("um_camera_direction", cameraSettings.CameraDirection);

        uint textureBindingPoint = 0;
        
        foreach (var value in values)
        {
            if (value.Value is Texture texture)
            {

                if (EngineVarContext.Global.GetBool("e_showmips", false))
                {
                    mipTexture ??= ResourceManager.Load<Texture>("tex/mipmap_display.dds");
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

    private void Load(ResourceManager resourceManager, string id, ContentContext content)
    {
        ResourceManager = resourceManager;
        
        Path = id;
        MaterialDefinition materialDefinition = LoadDef(resourceManager, id, content);

        if (materialDefinition.Shader == null)
            throw new Exception("No shader bound to material!");

        Shader = resourceManager.Load<Shader>(materialDefinition.Shader);

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
                    values[parameter.Name] = resourceManager.Load<Texture>(jsonValue.Value<string>()!);

            } else if (parameter.IsSampler)
            {
                values[parameter.Name] = resourceManager.Load<Texture>("__TEXTURE_AUTOGEN/white");
            }
        }
    }

    private MaterialDefinition LoadDef(ResourceManager resourceManager, string id, ContentContext content)
    {
        string json = content.LoadString(id);
        MaterialDefinition materialDefinition =
            JsonConvert.DeserializeObject<MaterialDefinition>(json);

        if (materialDefinition.Parent != null)
        {
            MaterialDefinition parent = LoadDef(resourceManager, materialDefinition.Parent, content);

            if (materialDefinition.Shader != null)
                parent.Shader = materialDefinition.Shader;

            foreach (var value in materialDefinition.Values)
            {
                parent.Values[value.Key] = value.Value;
            }

            materialDefinition = parent;
        }

        return materialDefinition;

    }
    
    
    private struct MaterialDefinition
    {
        [JsonProperty("parent")] public string? Parent;
        [JsonProperty("shader")] public string? Shader;
        [JsonProperty("values")] public JObject Values;
    }
    
    public class Loader : IResourceLoader<Material>
    {
        public Material LoadResource(ResourceManager resourceManager, string id, ContentContext content)
        {
            Material material = new Material();
            material.Load(resourceManager, id, content);
            return material;
        }
    }
}