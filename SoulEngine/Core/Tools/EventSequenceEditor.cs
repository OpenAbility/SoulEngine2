using Hexa.NET.ImGui;

namespace SoulEngine.Core.Tools;

public class EventSequenceEditor : EditorTool
{
    public EventSequenceEditor(Game game, Workspace workspace) : base(game, workspace)
    {
    }

    public override void Perform()
    {
        if (ImGui.Begin("Event Sequence Editor##" + ID, ref Enabled, ImGuiWindowFlags.MenuBar))
        {
            
            
            
        }
    }
}