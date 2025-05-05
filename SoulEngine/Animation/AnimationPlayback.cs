using SoulEngine.Core;

namespace SoulEngine.Animation;

public sealed class AnimationPlayback : EngineObject
{
    public float Weight => 1.0f;

    public bool Looping
    {
        get => context.Looping;
        set => context.Looping = value;
    }
    
    public TimeSpan Elapsed => context.Elapsed;

    public double TimeScale
    {
        get => context.Timescale;
        set => context.Timescale = value;
    }

    public AnimationClip Clip => context.Clip;
    public AnimationContext Context => context;
    
    
    private readonly AnimationPlayer player;
    private readonly AnimationContext context;
    
    public AnimationPlayback(AnimationPlayer animationPlayer, AnimationClip clip)
    {
        this.player = animationPlayer;
        this.context = new AnimationContext(clip, animationPlayer);
    }

    public void Play() => context.Play();
    public void Pause() => context.Pause();
    public void Stop() => context.Stop();
    public void Restart() => context.Restart();

    public void Delete()
    {
        player.RemovePlayback(this);
    }


}