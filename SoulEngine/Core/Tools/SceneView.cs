using Hexa.NET.ImGui;
using SoulEngine.Entities;
using SoulEngine.Props;
using SoulEngine.Util;

namespace SoulEngine.Core.Tools;

public class SceneView : EditorTool
{
    public SceneView(Game game, Workspace workspace) : base(game, workspace)
    {
    }

    public override  void Perform()
    {
        if (ImGui.Begin("Scene View##" + ID, ref Enabled))
        {
            #if DEVELOPMENT
            if (Game.Scene == null)
            {
                ImGui.Text("No scene is loaded!");
            }
            else
            {
                bool hoveredButton = false;
                
                foreach (var entity in new List<Entity>(Game.Scene.Entities))
                {
                    
                    if (ImGuiUtil.ImageSelectable(entity.Icon, entity.Name + "##" + entity.GetHashCode(), Workspace.CurrentEntity == entity))
                        Workspace.CurrentEntity = entity;

                    if (ImGui.IsItemHovered())
                        hoveredButton = true;
                    
                    
                    
                }

                if (!hoveredButton && ImGui.BeginPopupContextWindow())
                {
                    foreach (var type in EntityTemplateFactory.TemplateNames)
                    {
                        if (ImGui.Selectable(type))
                        {
                            EntityTemplateFactory.Initialize(type, type + " (" + Random.Shared.Next(1000, 9999) + ")", Game.Scene);
                        }
                    }
                    
                    
                    
                    ImGui.EndPopup();
                }
                
                
            }
#endif
        }
        ImGui.End();
    }
}