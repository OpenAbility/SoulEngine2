using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;
using Vector2 = System.Numerics.Vector2;

namespace SoulEngine.Rendering;

public class ImGuiWindow : EngineObject, IRenderSurface
{

    public string Name;

    private Framebuffer framebuffer;
    private readonly Game game;
    
    public Vector2 Position { get; private set; }
    
    
    public ImGuiWindow(Game game, string name)
    {
        Name = name;

        this.game = game;

        framebuffer = new Framebuffer(game, new Vector2i(1, 1));
    }

    public void Draw(bool padded, Action? beforeCallback, Action? afterCallback)
    {
        if(!padded)
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        Visible = false;
        if (ImGui.Begin(Name))
        {
            Active = ImGui.IsWindowFocused();
            
            Visible = true;
            if ((int)ImGui.GetContentRegionAvail().X != framebuffer.FramebufferSize.X || (int)ImGui.GetContentRegionAvail().Y != framebuffer.FramebufferSize.Y)
            {
                framebuffer = new Framebuffer(game, new Vector2i((int)ImGui.GetContentRegionAvail().X, (int)ImGui.GetContentRegionAvail().Y));
            }

            Position = ImGui.GetCursorScreenPos();
            
            beforeCallback?.Invoke();
            
            ImGui.Image(new ImTextureID(framebuffer.ColourBuffer), new Vector2(framebuffer.FramebufferSize.X, framebuffer.FramebufferSize.Y), new Vector2(0, 1), new Vector2(1, 0));
            
            afterCallback?.Invoke();
        }
        ImGui.End();
        if(!padded)
            ImGui.PopStyleVar();
    }
    
    public void BindFramebuffer()
    {
        framebuffer.BindFramebuffer();
    }

    public Vector2i FramebufferSize => framebuffer.FramebufferSize;
    public bool Visible { get; private set; }
    public bool Active { get; private set; }

    public int GetSurfaceHandle()
    {
        return framebuffer.Handle;
    }
}