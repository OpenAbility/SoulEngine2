using Hexa.NET.ImGui;

namespace SoulEngine.Core.Tools;

public class Inspector : EditorTool
{
    public Inspector(Game game, Workspace workspace) : base(game, workspace)
    {
    }

    public override void Perform()
    {
        if (ImGui.Begin("Inspector##" + ID, ref Enabled))
        {
#if DEVELOPMENT
            if (Workspace.CurrentEntity != null && Game.Scene != null)
            {
                Workspace.CurrentEntity.Edit();
                
                ImGui.Separator();
                if (ImGui.Button("Delete"))
                {
                    Game.Scene.Entities.Remove(Workspace.CurrentEntity);
                    Workspace.CurrentEntity = null;
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
}