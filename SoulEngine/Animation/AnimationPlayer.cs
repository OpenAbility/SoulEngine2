using SoulEngine.Models;

namespace SoulEngine.Animation;

public abstract class AnimationPlayer
{
    public readonly SkeletonInstance Skeleton;

    protected AnimationPlayer(SkeletonInstance skeleton)
    {
        Skeleton = skeleton;
    }

    public abstract void Apply();
}