using System.Diagnostics;
using OpenTK.Mathematics;
using SoulEngine.Models;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace SoulEngine.Animation;

public class AnimationContext
{

    private JointTranslation[] translations;
    private readonly SkeletonInstance skeletonInstance;
    private readonly AnimationPlayer animationPlayer;
    public readonly AnimationClip Clip;

    public TimeSpan Elapsed => timer.Elapsed;
    public bool Playing => timer.IsRunning;

    private ulong lastKeyframe;
    private Stopwatch timer = new Stopwatch();

    private int[] channelMappings;

    public bool Looping;
    
    public AnimationContext(AnimationClip clip, AnimationPlayer animationPlayer)
    {
        this.Clip = clip;

        SkeletonInstance instance = animationPlayer.Skeleton;
        this.animationPlayer = animationPlayer;
        
        skeletonInstance = instance;

        translations = new JointTranslation[instance.Skeleton.JointCount];
        for (int i = 0; i < instance.Skeleton.JointCount; i++)
        {
            translations[i] = new JointTranslation(instance.Skeleton.GetJoint(i));
        }

        channelMappings = new int[clip.ChannelCount];
        for (int i = 0; i < clip.ChannelCount; i++)
        {
            AnimationChannelInfo channel = this.Clip.GetChannel(i);
            channelMappings[i] = instance.Skeleton.GetJoint(channel.Name)?.SkeletonID ?? -1;
        }
    }

    public unsafe void Apply()
    {
        while (true)
        {
            if (lastKeyframe >= Clip.TotalKeyframes)
            {
                if(Looping)
                    Restart();
                else
                {
                    Pause();
                    break;
                }
            }

            AnimationKeyframe keyframe = Clip.GetKeyframe(lastKeyframe);
            if(keyframe.Timestamp > timer.Elapsed.TotalSeconds)
                break;
            lastKeyframe++;
            
            // TODO: Apply keyframes

            AnimationChannelInfo channelInfo = Clip.GetChannel(keyframe.Channel);

            if (channelInfo.Target == AnimationChannelTarget.Rotation)
                translations[channelMappings[keyframe.Channel]].Rotation = new Quaternion(keyframe.Data[0],
                    keyframe.Data[1], keyframe.Data[2], keyframe.Data[3]);
            else if (channelInfo.Target == AnimationChannelTarget.Translation)
                translations[channelMappings[keyframe.Channel]].Position = new Vector3(keyframe.Data[0],
                    keyframe.Data[1], keyframe.Data[2]);
            else if (channelInfo.Target == AnimationChannelTarget.Scale)
                translations[channelMappings[keyframe.Channel]].Scale = new Vector3(keyframe.Data[0],
                    keyframe.Data[1], keyframe.Data[2]);

        }

    }

    internal JointTranslation GetTranslation(int joint)
    {
        return translations[joint];
    }

    public void Play()
    {
        timer.Start();
    }
    
    public void Pause()
    {
        timer.Stop();
    }
    
    public void Stop()
    {
        timer.Reset();
    }
    
    public void Restart()
    {
        timer.Restart();
        lastKeyframe = 0;
    }
    
    internal struct JointTranslation(SkeletonJointData jointData)
    {
        public Vector3 Position = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Scale = Vector3.One;
        
        public readonly SkeletonJointData JointData = jointData;
        
        public Matrix4 Matrix => Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) *
                                 Matrix4.CreateTranslation(Position);
    }
}