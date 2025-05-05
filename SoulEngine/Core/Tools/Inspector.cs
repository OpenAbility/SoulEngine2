using Hexa.NET.ImGui;

namespace SoulEngine.Core.Tools;

public class Inspector : Game.Tool
{
    public override void Perform(Game game)
    {
        if (ImGui.Begin("Inspector", ref Enabled))
        {
#if DEVELOPMENT
            if (game.currentEntity != null && game.Scene != null)
            {
                game.currentEntity.Edit();
                
                ImGui.Separator();
                if (ImGui.Button("Delete"))
                {
                    game.Scene.Entities.Remove(game.currentEntity);
                    game.currentEntity = null;
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