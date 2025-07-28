using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SoulEngine.Development.ContentCompilers;

public class MaterialCompiler : ContentCompiler
{
    public override bool ShouldRecompile(ContentData contentData)
    {
        if (contentData.InputFile.LastWriteTimeUtc > contentData.OutputFile.LastWriteTimeUtc)
            return true;

        if (!contentData.Registry.Exists(contentData.FileDataPath("deps.count")))
            return true;

        int deps = contentData.Registry.GetInt32(contentData.FileDataPath("deps.count"));

        for (int i = 0; i < deps; i++)
        {
            if (File.GetLastWriteTimeUtc(contentData.Registry.GetString(contentData.FileDataPath("deps" + i))) >
                contentData.OutputFile.LastWriteTimeUtc)
                return true;
        }

        return false;
    }

    public override void Recompile(ContentData contentData)
    {
        List<string> deps = new List<string>();
        MaterialDefinition definition = LoadDefinition(contentData.InputFile.FullName, true, ref deps);
        
        contentData.Registry.SetInt32(contentData.FileDataPath("deps.count"), deps.Count);
        for (int i = 0; i < deps.Count; i++)
        {
            contentData.Registry.SetString(contentData.FileDataPath("deps" + i), deps[i]);
        }
        
        File.WriteAllText(contentData.OutputFile.FullName, JsonConvert.SerializeObject(definition, Formatting.None));
    }

    private MaterialDefinition LoadDefinition(string path, bool first, ref List<string> deps)
    {
        string json = File.ReadAllText(path);
        MaterialDefinition materialDefinition =
            JsonConvert.DeserializeObject<MaterialDefinition>(json);

        materialDefinition.Values ??= new JObject();
        
        if(!first)
            deps.Add(path);

        if (materialDefinition.Parent != null)
        {
            MaterialDefinition parent = LoadDefinition(Path.Join(Path.GetDirectoryName(path), materialDefinition.Parent), false, ref deps);

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

    public override string GetCompiledPath(string path)
    {
        return path;
    }
    
    private struct MaterialDefinition
    {
        [JsonProperty("parent")] public string? Parent;
        [JsonProperty("shader")] public string? Shader;
        [JsonProperty("values")] public JObject Values;
    }
}