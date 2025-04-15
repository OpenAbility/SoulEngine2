using Hexa.NET.ImGui;

namespace SoulEngine.Core.Tools;

public class Inspector : Game.Tool
{
    public override void Perform(Game game)
    {
        if (ImGui.Begin("Inspector", ref Enabled))
        {
            if (game.CurrentProp != null && game.Scene != null)
            {
                game.CurrentProp.Edit();
            }
            else
            {
                ImGui.Text("Select a prop to edit it!");
            }
        }
        ImGui.End();
    }

    public override string[] GetToolPath()
    {
        return ["Tools", "Scene", "Inspector"];
    }
}