using Hexa.NET.ImGui;

namespace SoulEngine.Core.Tools;

public class Inspector : Game.Tool
{
    public override void Perform(Game game)
    {
        if (ImGui.Begin("Inspector", ref Enabled))
        {
#if DEVELOPMENT
            if (game.CurrentProp != null && game.Scene != null)
            {
                game.CurrentProp.Edit();
                
                ImGui.Separator();
                if (ImGui.Button("Delete"))
                {
                    game.Scene.Props.Remove(game.CurrentProp);
                    game.CurrentProp = null;
                }
            }
            else
            {
                ImGui.Text("Select a prop to edit it!");
            }
#endif
        }
        ImGui.End();
    }

    public override string[] GetToolPath()
    {
        return ["Tools", "Scene", "Inspector"];
    }
}