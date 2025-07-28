using OpenAbility.Logging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Content;
using SoulEngine.Core;
using SoulEngine.Rendering;
using SoulEngine.Resources;

namespace SoulEngine.Compute;

[Resource("e.comp", typeof(ComputeShaderLoader))]
[ExpectedExtensions(".comp")]
public class ComputeShader : Resource
{
    private static readonly Logger Logger = Logger.Get<ComputeShader>();
    
    private readonly int handle;
    private readonly Game game;
    public Vector3i WorkGroupSize { get; private set; }

    
    private Dictionary<string, ShaderParameter> parameters = new Dictionary<string, ShaderParameter>();
    private Dictionary<string, uint> blocks = new Dictionary<string, uint>();

    public unsafe ComputeShader(Game game, int handle)
    {
        this.handle = handle;
        this.game = game;

        game.ThreadSafety.EnsureMain(() =>
        {
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
            
            int* groupSize = stackalloc int[3];
            GL.GetProgramiv(handle, ProgramProperty.ComputeWorkGroupSize, groupSize);
            WorkGroupSize = new Vector3i(groupSize[0], groupSize[1], groupSize[2]);
        });
    }

    public void Dispatch(Vector3i groups)
    {
        GL.UseProgram(handle);
        GL.DispatchCompute((uint)groups.X, (uint)groups.Y, (uint)groups.Z);
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

        if (!blocks.TryGetValue(name, out var index))
        {
            index = GL.GetProgramResourceIndex(handle, ProgramInterface.ShaderStorageBlock, name);
            blocks[name] = index;
        }
        GL.BindBufferRange(BufferTarget.ShaderStorageBuffer, index, buffer.Handle, offset * sizeof(T),
            size * sizeof(T));
        GL.ShaderStorageBlockBinding(handle, index, index);
    }
}

public class ComputeShaderLoader : IResourceLoader<ComputeShader>
{
    private static readonly Logger Logger = Logger.Get<ComputeShaderLoader>();
    
    public ComputeShader LoadResource(ResourceData data)
    {
        Game game = data.ResourceManager.Game;

        int handle = -1;
        
        game.ThreadSafety.EnsureMain(() =>
        {

            string source = data.ReadResourceString();
            source = ShaderProcessor.ProcessShader(data.Content, source, data.ResourcePath, null, game.RenderContext.SupportsLineDirectives);
            
            handle = GL.CreateShaderProgram(ShaderType.ComputeShader, source);
            
            if (GL.GetProgrami(handle, ProgramProperty.LinkStatus) != 1)
            {
                GL.GetProgramInfoLog(handle, out string programInfo);
                Logger.Error("Compute shader compile failed:\n{}", programInfo);
            }
        });

        return new ComputeShader(game, handle);
    }

    private string ProcessShaderSource(string source, string path)
    {
        return source;
    }
}