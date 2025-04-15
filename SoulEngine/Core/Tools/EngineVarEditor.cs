using Hexa.NET.ImGui;
using SoulEngine.Data;

namespace SoulEngine.Core.Tools;

public class EngineVarEditor : Game.Tool
{
    public override void Perform(Game game)
    {
        if (ImGui.Begin("Engine Vars", ref Enabled))
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

    public override string[] GetToolPath()
    {
        return ["Tools", "Engine Vars"];
    }
}