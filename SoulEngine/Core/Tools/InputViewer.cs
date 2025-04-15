using Hexa.NET.ImGui;
using SoulEngine.Input;

namespace SoulEngine.Core.Tools;

public class InputViewer : Game.Tool
{
    private bool showBuiltin;
    
    public override void Perform(Game game)
    {
        if (ImGui.Begin("Input Viewer", ref Enabled))
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

                foreach (var action in game.InputManager.Actions)
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

    public override string[] GetToolPath()
    {
        return ["Tools", "Input Viewer"];
    }
}