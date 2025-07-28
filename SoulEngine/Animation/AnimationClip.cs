using System.Runtime.InteropServices;
using System.Text;
using SoulEngine.Content;
using SoulEngine.Models;
using SoulEngine.Resources;
using SoulEngine.Util;

namespace SoulEngine.Animation;

[Resource("e.anim", typeof(Loader))]
[ExpectedExtensions(".anim")]
public unsafe class AnimationClip : Resource
{
    private readonly BinaryReader reader;

    public const uint Magic = 0x4D494E41;

    public readonly bool Streamed;
    public readonly int ChannelCount;
    public readonly ulong TotalKeyframes;

    private readonly long frameStartOffset;
 
    private readonly AnimationChannelInfo[] channels;

    private readonly AnimationKeyframe[] keyframes;
    
    public AnimationClip(Stream source) : this(new BinaryReader(source, Encoding.UTF8, true))
    {
    }
    
    public AnimationClip(BinaryReader reader)
    {
        this.reader = reader;
        
        uint magic = reader.ReadUInt32();
        if (magic != Magic)
            throw new Exception("Animation clip file corrupt!");

        uint version = reader.ReadUInt32();
        if (version != 1)
            throw new Exception("Unsupported animation file version!");

        Streamed = reader.ReadBoolean();

        ChannelCount = reader.ReadInt32();

        channels = new AnimationChannelInfo[ChannelCount];

        for (int i = 0; i < ChannelCount; i++)
        {
            string name = reader.ReadString();
            AnimationChannelTarget target = (AnimationChannelTarget)reader.ReadByte();
            AnimationChannelInterpolation interpolation = (AnimationChannelInterpolation)reader.ReadInt32();

            channels[i] = new AnimationChannelInfo(name, target, interpolation);
        }

        TotalKeyframes = reader.ReadUInt64();

        frameStartOffset = reader.BaseStream.Position;

        if (!Streamed)
        {
            keyframes = new AnimationKeyframe[TotalKeyframes];
            fixed (AnimationKeyframe* pointer = keyframes)
            {
                for (ulong i = 0; i < TotalKeyframes; i++)
                {
                    var targetSpan = new Span<byte>(&pointer[i], sizeof(AnimationKeyframe));
                    int read = 0;
                    while ((read = reader.Read(targetSpan)) != 0)
                        targetSpan = targetSpan.Slice(read);
                }
            }
        }
        else
        {
            keyframes = [];
        }
    }

    private readonly byte[] readBuffer = new byte[sizeof(AnimationKeyframe)];

    private AnimationKeyframe StreamKeyframe(ulong index)
    {
        if (index >= TotalKeyframes)
            throw new IndexOutOfRangeException();

        if (frameStartOffset + (long)index * sizeof(AnimationKeyframe) + sizeof(AnimationKeyframe) > reader.BaseStream.Length)
            throw new EndOfStreamException();
        
        reader.BaseStream.Position = frameStartOffset + (long)index * sizeof(AnimationKeyframe);

        if (reader.Read(readBuffer) != readBuffer.Length)
            throw new EndOfStreamException();
        
        return ((Span<byte>)readBuffer).CastStruct<AnimationKeyframe, byte>();
    }

    public AnimationKeyframe GetKeyframe(ulong index)
    {
        if (Streamed)
            return StreamKeyframe(index);
        
        if (index >= TotalKeyframes)
            throw new IndexOutOfRangeException();
        return keyframes[index];
    }

    public AnimationChannelInfo GetChannel(int index)
    {
        return channels[index];
    }

    ~AnimationClip()
    {
        reader.Close();
    }
    
    
    public class Loader : IResourceLoader<AnimationClip>
    {
        public AnimationClip LoadResource(ResourceData data)
        {
            return new AnimationClip(data.ResourceStream);
        }
    }
}

public struct AnimationChannelInfo(
    string name,
    AnimationChannelTarget target,
    AnimationChannelInterpolation interpolation)
{
    public readonly string Name = name;
    public readonly AnimationChannelTarget Target = target;
    public readonly AnimationChannelInterpolation Interpolation = interpolation;
}

[StructLayout(LayoutKind.Sequential, Pack = 0, CharSet = CharSet.Ansi)]
public unsafe struct AnimationKeyframe(int channel, float timestamp)
{
    public int Channel = channel;
    public float Timestamp = timestamp;
    public fixed float FromData[4];
    public fixed float ToData[4];
    public float Duration;
}

public enum AnimationChannelTarget
{
    Rotation = 0,
    Translation = 1,
    Scale = 2
}

public enum AnimationChannelInterpolation
{
    Linear = 0,
    Step = 1
}