using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;
using Vector2 = System.Numerics.Vector2;

namespace SoulEngine.Rendering;

public class ImGuiWindow : EngineObject
{

    public string Name;

    public Framebuffer Framebuffer { get; private set; }
    private readonly Game game;

    public bool Visible;
    public bool Active;
    
    public Vector2 Position { get; private set; }
    
    
    public ImGuiWindow(Game game, string name)
    {
        Name = name;

        this.game = game;

        Framebuffer = new Framebuffer(game, new Vector2i(1, 1));
    }

    public void Draw(bool padded, Action? beforeCallback, Action? afterCallback, ref bool closed)
    {
        if(!padded)
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        Visible = false;
        if (ImGui.Begin(Name, ref closed))
        {
            Active = ImGui.IsWindowFocused();
            
            Visible = true;
            if ((int)ImGui.GetContentRegionAvail().X != Framebuffer.FramebufferSize.X || (int)ImGui.GetContentRegionAvail().Y != Framebuffer.FramebufferSize.Y)
            {
                Framebuffer = new Framebuffer(game, new Vector2i((int)ImGui.GetContentRegionAvail().X, (int)ImGui.GetContentRegionAvail().Y));
            }

            Position = ImGui.GetCursorScreenPos();
            
            beforeCallback?.Invoke();
            
            ImGui.Image(new ImTextureID(Framebuffer.ColourBuffer), new Vector2(Framebuffer.FramebufferSize.X, Framebuffer.FramebufferSize.Y), new Vector2(0, 1), new Vector2(1, 0));
            
            afterCallback?.Invoke();
        }
        ImGui.End();
        if(!padded)
            ImGui.PopStyleVar();
    }
    
}