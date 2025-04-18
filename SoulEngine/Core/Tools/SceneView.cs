using Hexa.NET.ImGui;
using SoulEngine.Props;
using SoulEngine.Util;

namespace SoulEngine.Core.Tools;

public class SceneView : Game.Tool
{
    public override void Perform(Game game)
    {
        if (ImGui.Begin("Scene View", ref Enabled))
        {
            #if DEVELOPMENT
            if (game.Scene == null)
            {
                ImGui.Text("No scene is loaded!");
            }
            else
            {
                bool hoveredButton = false;
                
                foreach (var prop in new List<Prop>(game.Scene.Props))
                {
                    
                    if (ImGuiUtil.ImageSelectable(prop.propIcon, prop.Name + "##" + prop.GetHashCode(), game.CurrentProp == prop))
                        game.CurrentProp = prop;

                    if (ImGui.IsItemHovered())
                        hoveredButton = true;
                    
                    
                    
                }

                if (!hoveredButton && ImGui.BeginPopupContextWindow())
                {
                    foreach (var prop in PropLoader.Types)
                    {
                        if (ImGui.Selectable(prop))
                        {
                            game.Scene.AddProp(prop, prop + " (" + Random.Shared.Next(1000, 9999) + ")");
                        }
                    }
                    
                    ImGui.EndPopup();
                }
                
                
            }
#endif
        }
        ImGui.End();
    }

    public override string[] GetToolPath()
    {
        return ["Tools", "Scene", "Scene View"];
    }
}