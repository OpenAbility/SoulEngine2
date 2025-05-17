using System.Numerics;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using SoulEngine.Data.NBT;
using ImGuiWindow = SoulEngine.Rendering.ImGuiWindow;

namespace SoulEngine.Core.Tools;

public class SceneWindowTool : EditorTool
{
    public SceneWindowTool(Game game, Workspace workspace) : base(game, workspace)
    {
        Window = new ImGuiWindow(game, "Scene##" + ID);
        SceneCamera = new SceneCamera(game);
    }

    public ImGuiWindow Window;
    public SceneCamera SceneCamera;

    public override void OnLoad(CompoundTag tag)
    {
        Window = new ImGuiWindow(Game, "Scene##" + ID);
    }

    public override void Perform()
    {
#if DEVELOPMENT
        Workspace.SceneWindow = Window;
        Workspace.SceneCamera = SceneCamera;
        
        Window.Draw(false, () =>
        {
            ImGuizmo.SetDrawlist();
            Vector2 size = ImGui.GetContentRegionAvail();
            Vector2 position = ImGui.GetCursorScreenPos();
            ImGuizmo.SetRect(position.X, position.Y, size.X, size.Y);
        }, () =>
        {
            if (Game.Scene != null && Workspace.CurrentEntity != null)
            {
                Workspace.CurrentEntity.RenderMoveGizmo(Game.WorkspaceSceneCamera.GetView(), SceneCamera.GetProjection((float)Game.WorkspaceSceneWindow.FramebufferSize.X / Game.WorkspaceSceneWindow.FramebufferSize.Y));
            }
            
            SceneCamera.Update(Game.DeltaTime, ImGui.IsWindowFocused());
            
        }, ref Enabled);
#endif
    }
}