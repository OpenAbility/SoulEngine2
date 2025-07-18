using System.Collections;
using System.Diagnostics;
using System.Xml;
using OpenAbility.Logging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Content;
using SoulEngine.Core;
using SoulEngine.Resources;

namespace SoulEngine.Rendering;


[Resource("e.shader", typeof(Loader))]
[ExpectedExtensions(".program")]
public class Shader : Resource
{
    private Logger Logger = Logger.Get<Shader>();
    
    private int handle = -1;
    private Game? game;
    private Dictionary<string, ShaderParameter> parameters = new Dictionary<string, ShaderParameter>();
    private Dictionary<string, uint> blocks = new Dictionary<string, uint>();
    public Shader()
    {
        
    }

    public IEnumerable<ShaderParameter> Parameters => parameters.Values;

    
    
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
    
    private int UniformLocation(string name)
    {
        if (parameters.TryGetValue(name, out ShaderParameter value))
            return value.Location;
        return -1;
    }
    

    public void Matrix(string name, Matrix4 matrix, bool transpose)
    {
        int loc = UniformLocation(name);
        GL.ProgramUniformMatrix4f(handle, loc, 1, transpose, in matrix);
    }
    
    public void Uniform1i(string name, int value)
    {
        int loc = UniformLocation(name);
        GL.ProgramUniform1i(handle, loc, value);
    }
    
    public void Uniform1i(string name, int[] value)
    {
        int loc = UniformLocation(name);
        GL.ProgramUniform1i(handle, loc, value.Length, ref value[0]);
    }
    
    public void Uniform1f(string name, float value)
    {
        int loc = UniformLocation(name);
        GL.ProgramUniform1f(handle, loc, value);
    }
    public void Uniform2f(string name, Vector2 value)
    {
        int loc = UniformLocation(name);
        GL.ProgramUniform2f(handle, loc, value.X, value.Y);
    }
    
    public void Uniform2i(string name, Vector2i value)
    {
        int loc = UniformLocation(name);
        GL.ProgramUniform2i(handle, loc, value.X, value.Y);
    }
    
    public void Uniform3f(string name, Vector3 value)
    {
        int loc = UniformLocation(name);
        GL.ProgramUniform3f(handle, loc, value.X, value.Y, value.Z);
    }
    public void Uniform4f(string name, Vector4 value)
    {
        int loc = UniformLocation(name);
        GL.ProgramUniform4f(handle, loc, value.X, value.Y, value.Z, value.W);
    }

    public unsafe void BindBuffer<T>(string name, GpuBuffer<T> buffer, int offset, int size) where T : unmanaged
    {
        if(!blocks.TryGetValue(name, out var index))
            return;
        GL.BindBufferRange(BufferTarget.ShaderStorageBuffer, index, buffer.Handle, offset * sizeof(T),
            size * sizeof(T));
    }
    

    private void Load(ResourceManager resourceManager, string id, ContentContext content)
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
        
        foreach (XmlNode _child in shaderElement.ChildNodes)
        {
            if (_child is not XmlElement child)
                continue;
            
            if(child.Name != "Bindings")
                continue;
            
            if(child.GetAttribute("backend") != "SHARED" && child.GetAttribute("backend") != EngineData.Renderer)
                continue;

            foreach (XmlElement binding in child.ChildNodes)
            {
                string name = binding.GetAttribute("name");
                if (name.Length == 0)
                    throw new Exception("Invalid shader XML: Define has no name!");
                blocks[name] = UInt32.Parse(binding.InnerText);
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
        
        string prefix = "#version 450 core\n\n" + string.Join("\n", defines.Select(kvp => "#define " + kvp.Key + " " + kvp.Value)) + "\n";

        if (game.RenderContext.SupportsLineDirectives)
        {
            prefix += "#extension GL_ARB_shading_language_include : require\n#line 0 \"" + id + "\"\n";
        }
        else
        {
            prefix += "#line 0 0\n";
        }

        vertexSource = ShaderProcessor.ProcessShader(vertexSource ?? "", id + "/vertex", defines, game.RenderContext.SupportsLineDirectives);
        fragmentSource =ShaderProcessor.ProcessShader(fragmentSource ?? "", id + "/fragment", defines, game.RenderContext.SupportsLineDirectives);
        
        game.ThreadSafety.EnsureMain(() =>
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
            
            int uniformCount = GL.GetProgrami(handle, ProgramProperty.ActiveUniforms);
            for (int i = 0; i < uniformCount; i++)
            {

                GL.GetActiveUniform(handle, (uint)i, 2000, out int _, out int size, out UniformType type,
                    out string uniformName);
                
                if (!Enum.IsDefined(typeof(ShaderParameterType), (int)type))
                {
                    Logger.Error("Unsupported uniform type found! Uniform {}, Size: {}, Type: {}, Name: {}", i, size, type, uniformName);
                    continue;
                }
                
                ShaderParameter parameter = new ShaderParameter(uniformName, GL.GetUniformLocation(handle, uniformName), size, (ShaderParameterType)type);
                parameters[uniformName] = parameter;
            }
        });
    }
    
    public class Loader : IResourceLoader<Shader>
    {
        public Shader LoadResource(ResourceData data)
        {
            Shader shader = new Shader();
            // TODO: Redo shader loading
            shader.Load(data.ResourceManager, data.ResourcePath, data.Content);
            return shader;
        }
    }
}