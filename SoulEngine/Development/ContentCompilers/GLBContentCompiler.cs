using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;
using SoulEngine.Development.GLTF;
using SoulEngine.Rendering;
using SoulEngine.Util;

namespace SoulEngine.Development.ContentCompilers;

public abstract class GLBContentCompiler : ContentCompiler
{

    protected string ResolvePath(string source, string glb)
    {
        return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(source) ?? "", glb));
    }

    public override bool ShouldRecompile(ContentData contentData)
    {
        if (contentData.InputFile.LastWriteTimeUtc >= contentData.LastOutputWrite)
            return true;

        string glbKey = contentData.FileDataPath("glb_path");

        if (contentData.Registry.Exists(glbKey))
            return File.GetLastWriteTime(contentData.Registry.GetString(glbKey)) >= contentData.LastOutputWrite;

        JObject modelDef = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(contentData.InputFile.FullName))!;

        string glbPath = ResolvePath(contentData.InputFile.FullName, modelDef["glb"].Value<string>());
        contentData.Registry.SetString(glbKey, glbPath);
        
        return File.GetLastWriteTime(glbPath) >= contentData.LastOutputWrite;
    }
    
}