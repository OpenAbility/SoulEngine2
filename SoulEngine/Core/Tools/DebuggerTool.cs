using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using Vector4 = System.Numerics.Vector4;

namespace SoulEngine.Core.Tools;

public class DebuggerTool : EditorTool
{

    private const int FrameHistory = 60 * 5;

    private float maxFrameValue = 0;
    
    private readonly Dictionary<string, List<int>> historyMap = new Dictionary<string, List<int>>();

    public DebuggerTool(Game game, Workspace workspace) : base(game, workspace)
    {
    }

    public override void Perform()
    {
        using var debuggerSegment = Profiler.Instance.Segment("debugger");
        
        HashSet<string> activeSegments = new HashSet<string>(Profiler.Instance.Segments.Concat(historyMap.Keys));
        
        foreach (var segment in activeSegments)
        {
            historyMap.TryAdd(segment, new List<int>());

            int elapsed = (int)Profiler.Instance.GetTime(segment).TotalMilliseconds;
            historyMap[segment].Add(elapsed);

            if (elapsed > maxFrameValue)
                maxFrameValue = elapsed;

            if (historyMap[segment].Count > FrameHistory)
                historyMap[segment].RemoveAt(0);
        }

        maxFrameValue -= Game.Current.DeltaTime;
        
        if (ImGui.Begin("Frame Graph##" + ID))
        {
            if (ImPlot.BeginPlot("Frame Graph"))
            {
                ImPlot.SetupAxes((string)null!, "ms", ImPlotAxisFlags.NoTickLabels, ImPlotAxisFlags.NoTickLabels);
                ImPlot.SetupAxisLimits(ImAxis.X1, 0, FrameHistory, ImPlotCond.Always);
                ImPlot.SetupAxisLimits(ImAxis.Y1, 0, maxFrameValue * 1.5f, ImPlotCond.Always);

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
        
        
        if (ImGui.Begin("Debug##" + ID, ref Enabled))
        {
            ImGui.Text("Window Offset: " + Game.InputManager.WindowOffset);
            ImGui.Text("Window Size: " + Game.InputManager.WindowSize);
            ImGui.Text("Cursor Pos: " + Game.InputManager.MousePosition);
            ImGui.Text("Cursor Raw: " + Game.InputManager.RawMousePosition);
            ImGui.Text("Cursor Inside: " + Game.InputManager.MouseInWindow);
            ImGui.Text("Cursor Captured: " + Game.MainWindow.MouseCaptured);

            ImGui.Text("ImGui Pos: " + ImGui.GetMousePos());
            ImGui.Text("ImGui Clicked: " + ImGui.GetIO().MouseClicked[0]);
            
        }
        ImGui.End();
    }
}