using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using OpenTK.Mathematics;
using SharpFont;
using SoulEngine.Input;
using Encoding = System.Text.Encoding;
using Vector4 = System.Numerics.Vector4;

namespace SoulEngine.Core.Tools;

public class DebuggerTool : Game.Tool
{
    public DebuggerTool()
    {
        Enabled = true;
    }

    private const int FrameHistory = 60 * 5;
    
    
    private readonly Dictionary<string, List<int>> historyMap = new Dictionary<string, List<int>>();  

    public override void PerformAlways(Game game)
    {
        using var debuggerSegment = Profiler.Instance.Segment("debugger");
        
        HashSet<string> activeSegments = new HashSet<string>(Profiler.Instance.Segments.Concat(historyMap.Keys));

        foreach (var segment in activeSegments)
        {
            historyMap.TryAdd(segment, new List<int>());
            historyMap[segment].Add((int)Profiler.Instance.GetTime(segment).TotalMilliseconds);

            if (historyMap[segment].Count > FrameHistory)
                historyMap[segment].RemoveAt(0);
        }

    }

    public override void Perform(Game game)
    {
        if (ImGui.Begin("Debug", ref Enabled))
        {
            ImGui.Text("Window Offset: " + game.InputManager.WindowOffset);
            ImGui.Text("Window Size: " + game.InputManager.WindowSize);
            ImGui.Text("Cursor Pos: " + game.InputManager.MousePosition);
            ImGui.Text("Cursor Raw: " + game.InputManager.RawMousePosition);
            ImGui.Text("Cursor Inside: " + game.InputManager.MouseInWindow);
            ImGui.Text("Cursor Captured: " + game.MainWindow.MouseCaptured);

            ImGui.Text("ImGui Pos: " + ImGui.GetMousePos());
            ImGui.Text("ImGui Clicked: " + ImGui.GetIO().MouseClicked[0]);

            if (ImPlot.BeginPlot("Frame Graph"))
            {
                ImPlot.SetupAxes((string)null!, "ms", ImPlotAxisFlags.NoTickLabels, ImPlotAxisFlags.NoTickLabels);
                ImPlot.SetupAxisLimits(ImAxis.X1, 0, FrameHistory, ImPlotCond.Always);
                ImPlot.SetupAxisLimits(ImAxis.Y1, 0, 1000 / 10f, ImPlotCond.Always);

                ImPlot.SetNextFillStyle(new Vector4(0, 0, 0, -1), 0.5f);
                foreach (var segment in historyMap)
                {
                    var list = segment.Value.ToArray();
                    ImPlot.PlotLine(segment.Key, ref list[0], segment.Value.Count);
                }
                
                ImPlot.EndPlot();
            }
        }
        ImGui.End();
    }

    public override string[] GetToolPath()
    {
        return ["Tools", "Debugger"];
    }
}