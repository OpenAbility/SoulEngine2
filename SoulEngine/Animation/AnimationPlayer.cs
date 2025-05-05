using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Models;

namespace SoulEngine.Animation;

public sealed class AnimationPlayer : EngineObject
{
    public readonly SkeletonInstance Skeleton;

    private readonly List<AnimationPlayback> playbacks = new List<AnimationPlayback>();
    private readonly JointTranslation[] translations;

    public AnimationPlayer(SkeletonInstance skeleton)
    {
        Skeleton = skeleton;
        translations = new JointTranslation[skeleton.Skeleton.JointCount];

        for (int i = 0; i < skeleton.Skeleton.JointCount; i++)
        {
            translations[i] = new JointTranslation(skeleton.Skeleton.GetJoint(i));
        }
        
    }

    public void Apply()
    {
        for (int i = 0; i < translations.Length; i++)
        {
            translations[i].Reset();
        }

        foreach (var playback in playbacks)
        {
            playback.Context.Apply();
            
            for (int i = 0; i < translations.Length; i++)
            {
                if(!playback.Context.HasAnimated(i))
                    continue;
                
                JointTranslation playbackTranslation = playback.Context.GetTranslation(i);

                if (playbackTranslation.HasPosition)
                {
                    translations[i].Position =
                        Vector3.Lerp(translations[i].Position, playbackTranslation.Position, playback.Weight);
                }

                if (playbackTranslation.HasRotation)
                {
                    translations[i].Rotation =
                        Quaternion.Slerp(translations[i].Rotation, playbackTranslation.Rotation, playback.Weight);
                }

                if (playbackTranslation.HasScale)
                {
                    translations[i].Scale =
                        Vector3.Lerp(translations[i].Scale, playbackTranslation.Scale, playback.Weight);
                }

            }
        }
        
        for (int i = 0; i < translations.Length; i++)
        {
            Skeleton.TranslateJoint(translations[i].JointData, translations[i].Matrix);
        }
    }

    public AnimationPlayback Play(AnimationClip clip)
    {
        AnimationPlayback playback = new AnimationPlayback(this, clip);
        playbacks.Add(playback);
        playback.Play();
        return playback;
    }

    public bool Playing(AnimationClip clip)
    {
        return playbacks.Any(playback => playback.Clip == clip);
    }
    
    public bool Playing(string clip)
    {
        return playbacks.Any(playback => playback.Clip.ResourceID == clip);
    }

    public void StopAll()
    {
        playbacks.Clear();
    }

    internal void RemovePlayback(AnimationPlayback playback)
    {
        playbacks.Remove(playback);
    }
}