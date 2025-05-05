using System.Diagnostics;
using SoulEngine.Core;

namespace SoulEngine;

public class Profiler : EngineObject
{
    public static Profiler Instance = new Profiler();

    public Profiler()
    {
        
    }

    private Dictionary<string, TimeSpan> measuredTimespans = new Dictionary<string, TimeSpan>();
    private Dictionary<string, TimeSpan> lastFrameBuffer = new Dictionary<string, TimeSpan>();

    public void SubmitTime(string segment, TimeSpan timeSpan)
    {
        measuredTimespans.TryAdd(segment, TimeSpan.Zero);
        measuredTimespans[segment] += timeSpan;
    }

    public ProfilerSegment Segment(string name)
    {
        return new ProfilerSegment(this, name);
    }

    public TimeSpan GetTime(string segment)
    {
        return lastFrameBuffer.GetValueOrDefault(segment, TimeSpan.Zero);
    }

    public IEnumerable<string> Segments => measuredTimespans.Keys;

    public void Reset()
    {
        (measuredTimespans, lastFrameBuffer) = (lastFrameBuffer, measuredTimespans);
        
        foreach (var timespan in measuredTimespans.Keys)
        {
            measuredTimespans[timespan] = TimeSpan.Zero;
        }

        
    }
}

public class ProfilerSegment : IDisposable
{
    private readonly Profiler profiler;
    private readonly string segmentName;
    private readonly Stopwatch stopwatch;

    internal ProfilerSegment(Profiler profiler, string name)
    {
        this.profiler = profiler;
        this.segmentName = name;
        
        stopwatch = Stopwatch.StartNew();
    }
    
    public void Dispose()
    {
        profiler.SubmitTime(segmentName, stopwatch.Elapsed);
    }
}