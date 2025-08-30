using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace SoulEngine.Rendering;

public class ShaderBinder
{
    public ShaderBinder()
    {
        
    }


    private Shader? currentShader;
    
    private Dictionary<int, uint> textureBindingPoints = new Dictionary<int, uint>();
    private HashSet<uint> activeBindingPoints = new HashSet<uint>();
    


    private uint bindPoint = 0;
    private Dictionary<string, object> uniformValues = new Dictionary<string, object>();

    public void ClearTextures()
    {
        activeBindingPoints.Clear();
    }

    public void BindShader(Shader shader)
    {
        if (currentShader != shader)
        {
            uniformValues.Clear();
        }

        currentShader = shader;
        shader.Bind();
        ClearTextures();
    }

    public void BindTexture(string uniform, Texture texture) => BindTexture(uniform, texture.Handle);

    public void BindTexture(string uniform, int texture)
    {
        if (currentShader == null)
            throw new Exception("No shader bound!");

        uint bindingPoint = FindBindPoint(texture);
        
        GL.BindTextureUnit(bindingPoint, texture);

        activeBindingPoints.Add(bindingPoint);
        textureBindingPoints[texture] = bindingPoint;
        currentShader!.Uniform1i(uniform, (int)bindingPoint);
    }

    private uint FindBindPoint(int texture)
    {
        if (textureBindingPoints.TryGetValue(texture, out var bindingPoint))
            return bindingPoint;

        for (uint i = 0; i < 32; i++)
        {
            if (!textureBindingPoints.ContainsValue(i))
            {
                return i;
            }
        }
        
        for (uint i = 0; i < 32; i++)
        {
            if (!activeBindingPoints.Contains(i))
            {
                return i;
            }
        }

        throw new Exception("More than 32 textures bound!");
    }

    private bool CheckUniform(string uniform, object value)
    {
        return false;
        
        if (uniformValues.TryGetValue(uniform, out var existing))
            return Equals(existing, value);
        return true;
    }

    public void BindUniform(string uniform, int value)
    {
        if(CheckUniform(uniform, value)) return;
        currentShader!.Uniform1i(uniform, value);
    }
    
    public void BindUniform(string uniform, int[] value)
    {
        if(CheckUniform(uniform, value)) return;
        currentShader!.Uniform1i(uniform, value);
    }
    
    public void BindUniform(string uniform, float value)
    {
        if(CheckUniform(uniform, value)) return;
        currentShader!.Uniform1f(uniform, value);
    }
    
    public void BindUniform(string uniform, Vector2 value)
    {
        if(CheckUniform(uniform, value)) return;
        currentShader!.Uniform2f(uniform, value);
    }
    
    public void BindUniform(string uniform, Vector2i value)
    {
        if(CheckUniform(uniform, value)) return;
        currentShader!.Uniform2i(uniform, value);
    }
    
    public void BindUniform(string uniform, Vector3 value)
    {
        if(CheckUniform(uniform, value)) return;
        currentShader!.Uniform3f(uniform, value);
    }
    
    public void BindUniform(string uniform, Vector4 value)
    {
        if(CheckUniform(uniform, value)) return;
        currentShader!.Uniform4f(uniform, value);
    }
    
    public void BindUniform(string uniform, Colour value)
    {
        if(CheckUniform(uniform, value)) return;
        currentShader!.Uniform4f(uniform, value);
    }
    
    public void BindUniform(string uniform, Matrix4 value, bool transpose)
    {
        if (transpose)
            value = value.Transposed();
        
        if(CheckUniform(uniform, value)) return;
        currentShader!.Matrix(uniform, value, false);
    }

    public void BindBuffer<T>(string name, GpuBuffer<T> buffer, int offset, int size) where T : unmanaged
    {
        if(CheckUniform(name, buffer)) return;
        currentShader!.BindBuffer(name, buffer, offset, size);
    }
    
}