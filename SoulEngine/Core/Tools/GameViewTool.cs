using SoulEngine.Data.NBT;
using SoulEngine.Rendering;

namespace SoulEngine.Core.Tools;

public class GameViewTool : EditorTool
{
    public ImGuiWindow Window;

    
    public GameViewTool(Game game, Workspace workspace) : base(game, workspace)
    {
        
    }

    public override void OnLoad(CompoundTag tag)
    {
        Window = new ImGuiWindow(Game, "Game##" + ID);
    }

    public override void Perform()
    {
        Workspace.GameWindow = Window;
        
        Window.Draw(false, null, null, ref Enabled);
    }
}