using System.Diagnostics;

namespace SoulEngine.Util;

/// <summary>
/// Modified version of the <see cref="Stopwatch"/> class that also supports changing the time scale
/// </summary>
public class ScalableStopwatch
{
    private long recordedTime = 0;
    private long segmentStart = 0;
    private bool running;
    private double timescale = 1.0f;
    
    private static readonly double TickFrequencyScale = 10000000.0 / (double) Stopwatch.Frequency;
    
    public ScalableStopwatch()
    {
        
    }

    /// <summary>
    /// Starts or resumes time measuring
    /// </summary>
    public void Start()
    {
        if(running)
            return;
        segmentStart = Stopwatch.GetTimestamp();
        running = true;
    }

    /// <summary>
    /// Stops or pauses time measuring
    /// </summary>
    public void Stop()
    {
        if(!running)
            return;
        recordedTime += Stopwatch.GetTimestamp() - segmentStart;
        running = false;
        
        if(recordedTime >= 0)
            return;
        recordedTime = 0;
    }

    /// <summary>
    /// Stops and resets the stopwatch
    /// </summary>
    public void Reset()
    {
        recordedTime = 0;
        running = false;
        segmentStart = 0;
    }

    /// <summary>
    /// Resets the stopwatch to 0 and starts measuring
    /// </summary>
    public void Restart()
    {
        recordedTime = 0;
        running = true;
        segmentStart = Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// The current timescale to apply (default to 1.0)
    /// </summary>
    public double Timescale
    {
        get => timescale;
        set
        {
            if (running)
            {
                recordedTime += (long)((Stopwatch.GetTimestamp() - segmentStart) * timescale);
            }

            segmentStart = Stopwatch.GetTimestamp();
            timescale = value;
        }
    }

    /// <summary>
    /// Returns the <see cref="Elapsed"/> time as a string
    /// </summary>
    /// <returns>The elapsed time string in the same format used by <see cref="TimeSpan.ToString()"/></returns>
    public override string ToString() => Elapsed.ToString();

    /// <summary>
    /// Get the total amount of time measured
    /// </summary>
    public TimeSpan Elapsed => new TimeSpan(GetElapsedDateTimeTicks());
    
    /// <summary>
    /// Get the total amount of time measured, in milliseconds
    /// </summary>
    public long ElapsedMilliseconds => GetElapsedDateTimeTicks() / 10000L;
    
    /// <summary>
    /// Get the total of time measured, in ticks
    /// </summary>
    public long ElapsedTicks => GetRawElapsedTicks();

    public bool IsRunning => running;
    
    private long GetRawElapsedTicks()
    {
        long elapsed = recordedTime;
        if (running)
        {
            long num = Stopwatch.GetTimestamp() - segmentStart;
            elapsed += (long)(num * timescale);
        }
        return elapsed;
    }
    
    private long GetElapsedDateTimeTicks()
    {
        return (long) ((double) GetRawElapsedTicks() * TickFrequencyScale);
    }
}