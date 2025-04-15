using Hexa.NET.ImGui;
using SoulEngine.Props;

namespace SoulEngine.Core.Tools;

public class DirectorTool : Game.Tool
{
    public override void Perform(Game game)
    {
        if (ImGui.Begin("Director", ref Enabled))
        {
            if (game.Scene == null)
            {
                ImGui.Text("No scene is loaded!");
            }
            else if (game.Scene.Director == null)
            {
                ImGui.Text("No director is loaded!");
            }
            else
            {
                ImGui.Text("Director - " + game.Scene.Director.Type);
                ImGui.Separator();

                game.Scene.Director.Edit();
            }

            if (game.Scene != null)
            {
                if (ImGui.BeginPopupContextWindow())
                {
                    ImGui.TextDisabled("Create new director");
                    foreach (var type in DirectorLoader.Types)
                    {
                        if (ImGui.Selectable(type))
                        {
                            game.Scene.Director = DirectorLoader.Create(game.Scene, type);
                        }
                    }

                    ImGui.EndPopup();
                }
            }

        }

        ImGui.End();
    }

    public override string[] GetToolPath()
    {
        return ["Tools", "Scene", "Director"];
    }
}