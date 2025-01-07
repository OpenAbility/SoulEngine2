using System.Diagnostics;
using System.Xml;
using OpenAbility.Logging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Content;
using SoulEngine.Core;
using SoulEngine.Resources;

namespace SoulEngine.Rendering;


public class Shader : Resource
{
    private Logger Logger = Logger.Get<Shader>();
    
    private int handle = -1;
    private Game? game;
    public Shader()
    {
        
    }
    
    
    public override async Task Load(ResourceManager resourceManager, string id, ContentContext content)
    {
        Logger = Logger.Get("Shader", id);
        
        game = resourceManager.Game;
        
        XmlDocument document = new XmlDocument();
        document.Load(content.Load(id)!);

        XmlElement? shaderElement = document.DocumentElement;
        
        if(shaderElement == null)
            throw new Exception("Invalid shader XML: No root object!");

        if (shaderElement.Name != "Shader")
            throw new Exception("Invalid shader XML: Wrong root object!");

        Dictionary<string, string> defines = new Dictionary<string, string>();

        foreach (XmlNode _child in shaderElement.ChildNodes)
        {
            if (_child is not XmlElement child)
                continue;
            
            if(child.Name != "Defines")
                continue;
            
            if(child.GetAttribute("backend") != "SHARED" && child.GetAttribute("backend") != EngineData.Renderer)
                continue;

            foreach (XmlElement define in child.ChildNodes)
            {
                string name = define.GetAttribute("name");
                if (name.Length == 0)
                    throw new Exception("Invalid shader XML: Define has no name!");
                defines[name] = define.InnerText;
            }
        }

        string? fragmentSource = null;
        string? vertexSource = null;

        
        XmlNodeList? vertexNodes = document.SelectNodes("Shader/Vertex");
        if (vertexNodes != null)
        {
            foreach (XmlNode node in vertexNodes)
            {
                if (node is not XmlElement vertexElement)
                    continue;
                
                if(vertexElement.GetAttribute("backend") != "SHARED" && vertexElement.GetAttribute("backend") != EngineData.Renderer)
                    continue;

                if (vertexElement.HasAttribute("src"))
                    vertexSource = content.LoadString(vertexElement.GetAttribute("src"));
                else
                    vertexSource = vertexElement.GetAttribute("src_string");
            }
        }
        
        XmlNodeList? fragmentNodes = document.SelectNodes("Shader/Fragment");
        if (fragmentNodes != null)
        {
            foreach (XmlNode node in fragmentNodes)
            {
                if (node is not XmlElement fragmentElement)
                    continue;
                
                if(fragmentElement.GetAttribute("backend") != "SHARED" && fragmentElement.GetAttribute("backend") != EngineData.Renderer)
                    continue;

                if (fragmentElement.HasAttribute("src"))
                    fragmentSource = content.LoadString(fragmentElement.GetAttribute("src"));
                else
                    fragmentSource = fragmentElement.GetAttribute("src_string");
            }
        }

        vertexSource = "#version 420 core\n\n" + string.Join("\n", defines.Select(kvp => "#define " + kvp.Key + " " + kvp.Value)) + "\n#line 0\n" +  vertexSource;
        fragmentSource = "#version 420 core\n\n" + string.Join("\n", defines.Select(kvp => "#define " + kvp.Key + " " + kvp.Value)) + "\n#line 0\n" + fragmentSource;


        await game.ThreadSafety.EnsureMainAsync(() =>
        {
            handle = GL.CreateProgram();
            int vertexHandle = GL.CreateShader(ShaderType.VertexShader);
            int fragmentHandle = GL.CreateShader(ShaderType.FragmentShader);
            
            GL.ShaderSource(vertexHandle, vertexSource);
            GL.ShaderSource(fragmentHandle, fragmentSource);
            
            GL.CompileShader(vertexHandle);
            GL.CompileShader(fragmentHandle);
            
            GL.AttachShader(handle, vertexHandle);
            GL.AttachShader(handle, fragmentHandle);
            
            GL.LinkProgram(handle);
            
            GL.DetachShader(handle, vertexHandle);
            GL.DetachShader(handle, fragmentHandle);

            if (GL.GetProgrami(handle, ProgramProperty.LinkStatus) != 1)
            {
                GL.GetProgramInfoLog(handle, out string programInfo);
                GL.GetShaderInfoLog(vertexHandle, out string vertexInfo);
                GL.GetShaderInfoLog(fragmentHandle, out string fragmentInfo);
                
                Logger.Error("Shader load failed!");
                Logger.Error("PRG: {}", programInfo);
                Logger.Error("VTX: {}", vertexInfo);
                Logger.Error("FRG: {}", fragmentInfo);
            }
            
            GL.DeleteShader(vertexHandle);
            GL.DeleteShader(fragmentHandle);
        });
    }
    
    public void Bind()
    {
        if (handle == -1)
            throw new Exception("Attempted to bound unloaded shader!");
        GL.UseProgram(handle);
    }

    ~Shader()
    {
        game?.ThreadSafety.EnsureMain(() =>
        {
            if(handle != -1)
                GL.DeleteProgram(handle);
            handle = -1;
        });

    }

    private Dictionary<string, int> locations = new Dictionary<string, int>();
    private int UniformLocation(string name)
    {
        if (locations.TryGetValue(name, out int value))
            return value;
        value = GL.GetUniformLocation(handle, name);
        locations[name] = value;
        return value;
    }

    public void Matrix(string name, Matrix4 matrix, bool transpose)
    {
        int loc = UniformLocation(name);
        GL.UniformMatrix4f(loc, 1, transpose, in matrix);
    }
    
    public void Uniform1i(string name, int value)
    {
        int loc = UniformLocation(name);
        GL.Uniform1i(loc, value);
    }
}