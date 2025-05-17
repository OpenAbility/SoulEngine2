using Hexa.NET.ImGui;
using SoulEngine.Props;

namespace SoulEngine.Core.Tools;

public class DirectorTool : EditorTool
{
    public DirectorTool(Game game, Workspace workspace) : base(game, workspace)
    {
    }

    public override void Perform()
    {
        if (ImGui.Begin("Director##" + ID, ref Enabled))
        {
            if (Game.Scene == null)
            {
                ImGui.Text("No scene is loaded!");
            }
            else if (Game.Scene.Director == null)
            {
                ImGui.Text("No director is loaded!");
            }
            else
            {
                ImGui.Text("Director - " + Game.Scene.Director.Type);
                ImGui.Separator();

                Game.Scene.Director.Edit();
            }

            if (Game.Scene != null)
            {
                if (ImGui.BeginPopupContextWindow())
                {
                    ImGui.TextDisabled("Create new director");
                    foreach (var type in DirectorLoader.Types)
                    {
                        if (ImGui.Selectable(type))
                        {
                            Game.Scene.Director = DirectorLoader.Create(Game.Scene, type);
                        }
                    }

                    ImGui.EndPopup();
                }
            }

        }

        ImGui.End();
    }
    
}