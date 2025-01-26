using OpenTK.Graphics.OpenGL;

namespace SoulEngine.Rendering;

public struct RenderPass()
{
    public string? Name;
    public IRenderSurface Surface;

    public FramebufferAttachmentSettings DepthStencilSettings;
    //public FramebufferAttachmentSettings StencilSettings;
    public FramebufferAttachmentSettings[] ColorSettings = [];
}

public struct FramebufferAttachmentSettings
{
    public AttachmentLoadOp LoadOp;
    public AttachmentStoreOp StoreOp;

    public FramebufferClearValue ClearValue;
}

public struct FramebufferClearValue()
{
    public Colour Colour;
    public float Depth = 1;
    public byte Stencil = 0;
}

public enum AttachmentLoadOp
{
    Load,
    Clear,
    DontCare
}

public enum AttachmentStoreOp
{
    Store,
    DontCare
}