using System.Diagnostics;
using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Models;
using SoulEngine.Util;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace SoulEngine.Animation;

public class AnimationContext : EngineObject
{

    private JointTranslation[] translations;
    public readonly AnimationClip Clip;

    public TimeSpan Elapsed => timer.Elapsed;
    public bool Playing => timer.IsRunning;

    public double Timescale
    {
        get => timer.Timescale;
        set => timer.Timescale = value;
    }

    
    private ScalableStopwatch timer = new ScalableStopwatch();

    private int[] channelMappings;
    private bool[] animatedJoints;
    private AnimationKeyframe[] lastKeyframes;
    

    public bool Looping;
    
    public AnimationContext(AnimationClip clip, AnimationPlayer animationPlayer)
    {
        Clip = clip;

        SkeletonInstance instance = animationPlayer.Skeleton;

        translations = new JointTranslation[instance.Skeleton.JointCount];
        for (int i = 0; i < instance.Skeleton.JointCount; i++)
        {
            translations[i] = new JointTranslation(instance.Skeleton.GetJoint(i));
        }

        animatedJoints = new bool[instance.Skeleton.JointCount];

        lastKeyframes = new AnimationKeyframe[clip.ChannelCount];
        for (int i = 0; i < clip.ChannelCount; i++)
        {
            lastKeyframes[i].Channel = -1;
        }
        
        channelMappings = new int[clip.ChannelCount];
        for (int i = 0; i < clip.ChannelCount; i++)
        {
            AnimationChannelInfo channel = this.Clip.GetChannel(i);
            channelMappings[i] = instance.Skeleton.GetJoint(channel.Name)?.SkeletonID ?? -1;
        }
    }
    
    private ulong lastKeyframeIndex;
    private readonly float[] workingBuffer = new float[4];
    
    

    public unsafe void Apply()
    {
        double currentTime = timer.Elapsed.TotalSeconds;

        // Poll keyframes into lastKeyframes
        while (true)
        {
            // If we've hit the end then we loop.
            if (lastKeyframeIndex >= Clip.TotalKeyframes)
            {
                if(Looping)
                    Restart();
                else
                {
                    Pause();
                    break;
                }
            }
            
            currentTime = timer.Elapsed.TotalSeconds;

            // Fetch the next keyframe, but only show it if it's not in the future
            AnimationKeyframe keyframe = Clip.GetKeyframe(lastKeyframeIndex);
            
            if (keyframe.Timestamp > currentTime)
            {
                break;
            }
            
            lastKeyframeIndex++;
            lastKeyframes[keyframe.Channel] = keyframe;
        }

        // Apply animations
        for (int i = 0; i < lastKeyframes.Length; i++)
        {
            AnimationKeyframe keyframe = lastKeyframes[i];
            
            // If the channel is -1 then it's an invalid keyframe - wait for future polling
            if(keyframe.Channel == -1)
                continue;

            // If the channel maps to -1 then it can't be applied on the current skeleton - skip
            if(channelMappings[keyframe.Channel] == -1)
                continue;
            
            // Calculate keyframe progress
            float progress = (float)(currentTime - keyframe.Timestamp) / keyframe.Duration;
            
            // Fetch the animation channel information - used for interpolation and application
            AnimationChannelInfo channelInfo = Clip.GetChannel(keyframe.Channel);

            // Interpolate the keyframes.
           
            if (channelInfo.Interpolation == AnimationChannelInterpolation.Step)
            {
                // Step is easy enough - just do a copy
                workingBuffer[0] = keyframe.FromData[0];
                workingBuffer[1] = keyframe.FromData[1];
                workingBuffer[2] = keyframe.FromData[2];
                workingBuffer[3] = keyframe.FromData[3];
            } else if (channelInfo.Interpolation == AnimationChannelInterpolation.Linear)
            {
                // Linear interpolation is different on rotations and scale etc
                
                if (channelInfo.Target == AnimationChannelTarget.Rotation)
                {
                    // For rotations, use Quaternions for the added Slerp functionality - this is as specified in the glTF spec
                    Quaternion a = new Quaternion(keyframe.FromData[0], keyframe.FromData[1], keyframe.FromData[2],
                        keyframe.FromData[3]);
                    Quaternion b = new Quaternion(keyframe.ToData[0], keyframe.ToData[1], keyframe.ToData[2],
                        keyframe.ToData[3]);

                    Quaternion result = Quaternion.Slerp(a, b, progress);
                    
                    workingBuffer[0] = result.X;
                    workingBuffer[1] = result.Y;
                    workingBuffer[2] = result.Z;
                    workingBuffer[3] = result.W;
                }
                else
                {
                    // Otherwise, just use Lerp on a Vector4
                    // This will leave garbage data in the W component as all regular lerps are done on 3-component vectors.
                    // We don't read it, so we're fine
                    Vector4 a = new Vector4(keyframe.FromData[0], keyframe.FromData[1], keyframe.FromData[2],
                        keyframe.FromData[3]);
                    Vector4 b = new Vector4(keyframe.ToData[0], keyframe.ToData[1], keyframe.ToData[2],
                        keyframe.ToData[3]);

                    Vector4 result = Vector4.Lerp(a, b, progress);
                    
                    workingBuffer[0] = result.X;
                    workingBuffer[1] = result.Y;
                    workingBuffer[2] = result.Z;
                    workingBuffer[3] = result.W;
                }
            }
            
            // Apply the appropriate translation from the working buffer
            if (channelInfo.Target == AnimationChannelTarget.Rotation)
            {
                translations[channelMappings[keyframe.Channel]].Rotation = new Quaternion(workingBuffer[0],
                    workingBuffer[1], workingBuffer[2], workingBuffer[3]);

                translations[channelMappings[keyframe.Channel]].HasRotation = true;

            }
            else if (channelInfo.Target == AnimationChannelTarget.Translation)
            {
                translations[channelMappings[keyframe.Channel]].Position = new Vector3(workingBuffer[0],
                    workingBuffer[1], workingBuffer[2]);
                
                translations[channelMappings[keyframe.Channel]].HasPosition = true;
            }
            else if (channelInfo.Target == AnimationChannelTarget.Scale)
            {
                translations[channelMappings[keyframe.Channel]].Scale = new Vector3(workingBuffer[0],
                    workingBuffer[1], workingBuffer[2]);
                
                translations[channelMappings[keyframe.Channel]].HasScale = true;
            }

            animatedJoints[channelMappings[keyframe.Channel]] = true;
        }
        
    }

    internal JointTranslation GetTranslation(int joint)
    {
        return translations[joint];
    }

    internal bool HasAnimated(int joint)
    {
        return animatedJoints[joint];
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
        ResetState();
    }
    
    public void Restart()
    {
        timer.Restart();
        ResetState();
    }

    private void ResetState()
    {
        lastKeyframeIndex = 0;
        for (int i = 0; i < lastKeyframes.Length; i++)
        {
            lastKeyframes[i].Channel = -1;
        }
        
        for (int i = 0; i < animatedJoints.Length; i++)
        {
            animatedJoints[i] = false;
            translations[i].HasPosition = false;
            translations[i].HasRotation = false;
            translations[i].HasScale = false;
        }
    }
    
}