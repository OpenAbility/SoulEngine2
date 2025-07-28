using Hexa.NET.ImGui;
using SoulEngine.Data;

namespace SoulEngine.Core.Tools;

public class EngineVarEditor : EditorTool
{
    public EngineVarEditor(Game game, Workspace workspace) : base(game, workspace)
    {
    }

    public override void Perform()
    {
        if (ImGui.Begin("Engine Vars##" + ID, ref Enabled))
        {
            foreach (var variableName in EngineVarContext.Global.GetEntries())
            {
                EngineVarContext.EngineVarEntry entryData = EngineVarContext.Global.GetEntry(variableName)!.Value;

                ImGui.BeginDisabled(entryData.Locked);

                entryData.Value = Inspection.Inspector.Inspect(entryData.Value, entryData.Name, out bool edited)!;

                if (edited)
                {
                    EngineVarContext.Global.SetEntry(variableName, entryData);
                }

                ImGui.EndDisabled();
            }
        }

        ImGui.End();
    }
    
}