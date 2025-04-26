using System.Runtime.InteropServices;
using System.Text;
using SoulEngine.Content;
using SoulEngine.Models;
using SoulEngine.Resources;
using SoulEngine.Util;

namespace SoulEngine.Animation;

[Resource(typeof(Loader))]
public unsafe class AnimationClip : Resource
{
    private readonly BinaryReader reader;

    public const uint Magic = 0x4D494E41;

    public readonly bool Streamed;
    public readonly int ChannelCount;
    public readonly ulong TotalKeyframes;

    private readonly long frameStartOffset;
 
    private readonly AnimationChannelInfo[] channels;
    
    public AnimationClip(Stream source)
    {
        if(source == Stream.Null)
            return;
        
        reader = new BinaryReader(source, Encoding.UTF8, false);

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
    }

    private readonly byte[] readBuffer = new byte[sizeof(AnimationKeyframe)];

    public AnimationKeyframe GetKeyframe(ulong index)
    {
        if (index >= TotalKeyframes)
            throw new IndexOutOfRangeException();

        if (frameStartOffset + (long)index * sizeof(AnimationKeyframe) + sizeof(AnimationKeyframe) >= reader.BaseStream.Length)
            throw new EndOfStreamException();
        
        reader.BaseStream.Position = frameStartOffset + (long)index * sizeof(AnimationKeyframe);

        if (reader.Read(readBuffer) != readBuffer.Length)
            throw new EndOfStreamException();
        
        return ((Span<byte>)readBuffer).CastStruct<AnimationKeyframe, byte>();
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
        public AnimationClip LoadResource(ResourceManager resourceManager, string id, ContentContext content)
        {
            if (!content.Exists(id))
                return new AnimationClip(Stream.Null);
            
            return new AnimationClip(content.Load(id)!);
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