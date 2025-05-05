using Hexa.NET.ImGui;
using SoulEngine.Entities;
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
                
                foreach (var prop in new List<Entity>(game.Scene.Entities))
                {
                    
                    if (ImGuiUtil.ImageSelectable(prop.Icon, prop.Name + "##" + prop.GetHashCode(), game.currentEntity == prop))
                        game.currentEntity = prop;

                    if (ImGui.IsItemHovered())
                        hoveredButton = true;
                    
                    
                    
                }

                if (!hoveredButton && ImGui.BeginPopupContextWindow())
                {
                    foreach (var type in EntityTemplateFactory.TemplateNames)
                    {
                        if (ImGui.Selectable(type))
                        {
                            EntityTemplateFactory.Initialize(type, type + " (" + Random.Shared.Next(1000, 9999) + ")", game.Scene);
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