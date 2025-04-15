using Hexa.NET.ImGui;
using SoulEngine.Input;

namespace SoulEngine.Core.Tools;

public class DebuggerTool : Game.Tool
{
    public DebuggerTool()
    {
        Enabled = true;
    }
    
    public override void Perform(Game game)
    {
        if (ImGui.Begin("Debug", ref Enabled))
        {
            ImGui.Text("Window Offset: " + game.InputManager.WindowOffset);
            ImGui.Text("Window Size: " + game.InputManager.WindowSize);
            ImGui.Text("Cursor Pos: " + game.InputManager.MousePosition);
            ImGui.Text("Cursor Raw: " + game.InputManager.RawMousePosition);
            ImGui.Text("Cursor Inside: " + game.InputManager.MouseInWindow);
            ImGui.Text("Cursor Captured: " + game.MainWindow.MouseCaptured);

            ImGui.Text("ImGui Pos: " + ImGui.GetMousePos());
            ImGui.Text("ImGui Clicked: " + ImGui.GetIO().MouseClicked[0]);
        }
        ImGui.End();
    }

    public override string[] GetToolPath()
    {
        return ["Tools", "Debugger"];
    }
}