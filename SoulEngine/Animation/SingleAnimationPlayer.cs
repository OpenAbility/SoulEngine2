using SoulEngine.Models;

namespace SoulEngine.Animation;

public class SingleAnimationPlayer : AnimationPlayer
{
    public AnimationClip? CurrentClip
    {
        get => context?.Clip;
        set
        {
            if(CurrentClip == value)
                return;

            if (value == null)
            {
                context = null;
                return;
            }

            context = new AnimationContext(value, this);
        }
    }

    public AnimationContext? Playing => context;

    private AnimationContext? context;
    
    public SingleAnimationPlayer(SkeletonInstance skeleton) : base(skeleton)
    {
    }

    public override void Apply()
    {
        if(context == null)
            return;
        context.Apply();

        for (int i = 0; i < Skeleton.Skeleton.JointCount; i++)
        {
            // Sometimes we don't have valid animation data for a joint - this causes issues with the visuals.
            // So, we keep track of that and only apply animations when they exist
            if(!context.HasAnimated(i))
                continue;
            
            AnimationContext.JointTranslation translation = context.GetTranslation(i);
            
            Skeleton.TranslateJoint(translation.JointData, translation.Matrix);
        }
    }
}