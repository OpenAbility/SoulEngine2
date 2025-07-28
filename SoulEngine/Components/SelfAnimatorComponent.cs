using SoulEngine.Animation;
using SoulEngine.Core;
using SoulEngine.Entities;
using SoulEngine.Props;

namespace SoulEngine.Components;

[Component("animate_self")]
public class SelfAnimatorComponent : Component
{
    private AnimationPlayback? playback;
    
    [SerializedProperty("animation")]
    public AnimationClip? Clip
    {
        get
        {
            return playback?.Clip;
        }
        set
        {
            if (value == null)
            {
                playback?.Delete();
                playback = null;
                return;
            }

            playback = Entity.GetComponent<DynamicModelComponent>()?.AnimationPlayer?.Play(value);
            if(playback != null)
                playback.Looping = shouldLoop;
        }
    }

    [SerializedProperty("looping")]
    public bool Looping
    {
        get => shouldLoop;
        set
        {
            shouldLoop = value;
            if (playback != null)
                playback.Looping = shouldLoop;
        }
    }

    private bool shouldLoop = false;
    
    public SelfAnimatorComponent(Entity entity) : base(entity)
    {
    }
}