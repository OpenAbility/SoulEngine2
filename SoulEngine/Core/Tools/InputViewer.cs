using Hexa.NET.ImGui;
using SoulEngine.Input;

namespace SoulEngine.Core.Tools;

public class InputViewer : EditorTool
{

    private bool showBuiltin;
    
    public InputViewer(Game game, Workspace workspace) : base(game, workspace)
    {
    }

    public override void Perform()
    {
        if (ImGui.Begin("Input Viewer##" + ID, ref Enabled))
        {
            if (ImGui.BeginTable("Inputs", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
            {

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TableHeader("Name");
                ImGui.TableNextColumn();
                ImGui.TableHeader("Down");
                ImGui.TableNextColumn();
                ImGui.TableHeader("Pressed");
                ImGui.TableNextColumn();
                ImGui.TableHeader("Released");

                foreach (var action in Game.InputManager.Actions)
                {
                    if(action.Name.StartsWith("builtin.") && !showBuiltin)
                        continue;
                    ImGui.TableNextRow();
                        
                    ImGui.TableNextColumn();
                    ImGui.Text(action.Name);
                        
                    ImGui.TableNextColumn();
                    ImGui.Text(action.Down.ToString());
      
                    ImGui.TableNextColumn();
                    ImGui.Text(action.Pressed.ToString());
    
                    ImGui.TableNextColumn();
                    ImGui.Text(action.Released.ToString());
                }
                    
                ImGui.EndTable();
            }

            ImGui.Checkbox("Show builtin actions", ref showBuiltin);
        }

        ImGui.End();
    }
}