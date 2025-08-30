using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using Vector4 = System.Numerics.Vector4;

namespace SoulEngine.Core.Tools;

public class DebuggerTool : EditorTool
{

   
    public DebuggerTool(Game game, Workspace workspace) : base(game, workspace)
    {
    }

    public override void Perform()
    {
        if (ImGui.Begin("Debug##" + ID, ref Enabled))
        {
            ImGui.Text("Window Offset: " + Game.InputManager.WindowOffset);
            ImGui.Text("Window Size: " + Game.InputManager.WindowSize);
            ImGui.Text("Cursor Pos: " + Game.InputManager.MousePosition);
            ImGui.Text("Cursor Raw: " + Game.InputManager.RawMousePosition);
            ImGui.Text("Cursor Inside: " + Game.InputManager.MouseInWindow);
            ImGui.Text("Cursor Captured: " + Game.MainWindow.MouseCaptured);

            ImGui.Text("ImGui Pos: " + ImGui.GetMousePos());
            ImGui.Text("ImGui Clicked: " + ImGui.GetIO().MouseClicked[0]);
            
        }
        ImGui.End();
    }
}