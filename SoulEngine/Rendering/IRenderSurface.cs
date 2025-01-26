using OpenTK.Mathematics;

namespace SoulEngine.Rendering;

public interface IRenderSurface
{
    public void BindFramebuffer();
    
    public Vector2i FramebufferSize { get; }

    public int GetSurfaceHandle();
}