using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace SoulEngine.Rendering;

public static class RenderUtility  
{
    public static void Clear(Colour? colour, float? depth, int? stencil)
    {
        ClearBufferMask bufferMask = 0;
        if (colour != null)
        {
            GL.ClearColor(colour.Value.R, colour.Value.G, colour.Value.B, colour.Value.A);
            bufferMask |= ClearBufferMask.ColorBufferBit;
        }

        if (depth != null)
        {
            GL.ClearDepthf(depth.Value);
            bufferMask |= ClearBufferMask.DepthBufferBit;
        }

        if (stencil != null)
        {
            GL.ClearStencil(stencil.Value);
            bufferMask |= ClearBufferMask.StencilBufferBit;
        }
        
        GL.Clear(bufferMask);
    }
}