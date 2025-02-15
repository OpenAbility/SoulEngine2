using System.Text;
using Newtonsoft.Json;
using SoulEngine.Animation;
using SoulEngine.Development.GLTF;
using SoulEngine.Util;

namespace SoulEngine.Development.ContentCompilers;

public class AnimationCompiler : GLBContentCompiler
{
    public override unsafe void Recompile(ContentData contentData)
    {
        AnimDef modelDef = JsonConvert.DeserializeObject<AnimDef>(File.ReadAllText(contentData.InputFilePath));
        string glbPath = ResolvePath(contentData.InputFilePath, modelDef.Glb);

        GLTFLoader loader = new GLTFLoader(File.OpenRead(glbPath), false);

        GLTF.Animation animation = SelectAnimation(modelDef.Animation, loader.File);

        modelDef.AnimationName ??= modelDef.Animation;

        if (modelDef.AnimationName == null)
            throw new Exception("Animation needs a name!");

        using BinaryWriter writer = new BinaryWriter(File.OpenWrite(contentData.OutputFilePath), Encoding.UTF8, false);
        
        // Just a small header
        writer.Write(AnimationClip.Magic);
        
        // 32-bit version integer
        writer.Write(1);
        
        // Should this animation be streamed from disk or not?
        writer.Write(modelDef.Streamed);
        
        // Each animation file contains a stream of keyframes.
        // Each keyframe contains data for X amount of channels
        // Each channel maps to one component of one bone.
        
        // The channel data is interleaved so that we can stream it in the future
        
        // First up is the channels
        writer.Write(animation.Channels.Length);
        for (int i = 0; i < animation.Channels.Length; i++)
        {
            // First thing to write is the name of our bone
            writer.Write(loader.File.Nodes[animation.Channels[i].Target.Node].Name!);
            
            // Then we decode the "path" and write it as a byte index
            if(animation.Channels[i].Target.Path == "rotation")
                writer.Write((byte)0);
            else if (animation.Channels[i].Target.Path == "translation")
                writer.Write((byte)1);
            else if (animation.Channels[i].Target.Path == "scale")
                writer.Write((byte)2);
            else if (animation.Channels[i].Target.Path == "weights")
                writer.Write((byte)3);
            
            AnimationSampler sampler = animation.Samplers[animation.Channels[i].Sampler];
            
            int interp = sampler.Interpolation switch
            {
                "LINEAR" => 0,
                "STEP" => 1,
                _ => throw new Exception("Unsupported interpolation mode " + sampler.Interpolation)
            };
            
            writer.Write(interp);

        }
        
        // We then need to figure out how many keyframes are in the animation.
        ulong totalKeyFrames = 0;
        for (int i = 0; i < animation.Channels.Length; i++)
            totalKeyFrames += (ulong)loader.File.Accessors[animation.Samplers[animation.Channels[i].Sampler].Input].Count;

        writer.Write(totalKeyFrames);
        
        // Then comes the interleaving stream data
        // It's a bit of a shitshow, but we can make it work.
        // Just trust.

        bool foundFrames = true;
        const float step = 0.1f;
        float timestamp = 0;
        ulong writtenFrames = 0;

        int[] currentFrames = new int[animation.Channels.Length];
        
        List<KeyframeData> keyframeDatas = new List<KeyframeData>();
        
        while (foundFrames)
        {
            timestamp += step;
            
            // We iterate through each channel of each animation
            foundFrames = false;

            keyframeDatas.Clear();
            
            for (int i = 0; i < animation.Channels.Length; i++)
            {
                AnimationSampler sampler = animation.Samplers[animation.Channels[i].Sampler];
                Accessor frameAccessor = loader.File.Accessors[sampler.Input];

                Accessor dataAccessor = loader.File.Accessors[sampler.Output];

   
                while (true)
                {
                    if(currentFrames[i] >= frameAccessor.Count)
                        break;
                    
                    float frameTs = loader.GetAccessor(frameAccessor, currentFrames[i]).CastStruct<float, byte>();
                    // It's within the current step

                    if (frameTs >= timestamp)
                        foundFrames = true;
                
                    if (frameTs > timestamp || frameTs < timestamp - step)
                        break;

                    KeyframeData keyframeData = new KeyframeData()
                    {
                        Channel = i,
                        Data = [0, 0, 0, 0],
                        Timestamp = frameTs
                    };

                    if (animation.Channels[i].Target.Path == "rotation")
                    {
                        keyframeData.Data[0] = loader.GetAccessor(dataAccessor, currentFrames[i] * 4 + 0).CastStruct<float, byte>();
                        keyframeData.Data[1] = loader.GetAccessor(dataAccessor, currentFrames[i] * 4 + 1).CastStruct<float, byte>();
                        keyframeData.Data[2] = loader.GetAccessor(dataAccessor, currentFrames[i] * 4 + 2).CastStruct<float, byte>();
                        keyframeData.Data[3] = loader.GetAccessor(dataAccessor, currentFrames[i] * 4 + 3).CastStruct<float, byte>();
                    }
                    if (animation.Channels[i].Target.Path == "translation")
                    {
                        keyframeData.Data[0] = loader.GetAccessor(dataAccessor, currentFrames[i] * 3 + 0).CastStruct<float, byte>();
                        keyframeData.Data[1] = loader.GetAccessor(dataAccessor, currentFrames[i] * 3 + 1).CastStruct<float, byte>();
                        keyframeData.Data[2] = loader.GetAccessor(dataAccessor, currentFrames[i] * 3 + 2).CastStruct<float, byte>();
                    }
                    if (animation.Channels[i].Target.Path == "scale")
                    {
                        keyframeData.Data[0] = loader.GetAccessor(dataAccessor, currentFrames[i] * 3 + 0).CastStruct<float, byte>();
                        keyframeData.Data[1] = loader.GetAccessor(dataAccessor, currentFrames[i] * 3 + 1).CastStruct<float, byte>();
                        keyframeData.Data[2] = loader.GetAccessor(dataAccessor, currentFrames[i] * 3 + 2).CastStruct<float, byte>();
                    }
                    if (animation.Channels[i].Target.Path == "weights")
                    {
                        keyframeData.Data[0] = loader.GetAccessor(dataAccessor, currentFrames[i] * 4 + 0).CastStruct<float, byte>();
                        keyframeData.Data[1] = loader.GetAccessor(dataAccessor, currentFrames[i] * 4 + 1).CastStruct<float, byte>();
                        keyframeData.Data[2] = loader.GetAccessor(dataAccessor, currentFrames[i] * 4 + 2).CastStruct<float, byte>();
                        keyframeData.Data[3] = loader.GetAccessor(dataAccessor, currentFrames[i] * 4 + 3).CastStruct<float, byte>();
                    }

                    keyframeDatas.Add(keyframeData);

                    currentFrames[i]++;

                }
                
                
            }
            
            keyframeDatas.Sort((a, b) => Math.Sign(a.Timestamp - b.Timestamp));

            for (int i = 0; i < keyframeDatas.Count; i++)
            {

                AnimationKeyframe keyframe =
                    new AnimationKeyframe(keyframeDatas[i].Channel, keyframeDatas[i].Timestamp);
                
                for (int j = 0; j < keyframeDatas[i].Data.Length; j++)
                {
                    keyframe.Data[j] = keyframeDatas[i].Data[j];
                }
                
                writer.Write(new ReadOnlySpan<byte>((byte*)&keyframe, sizeof(AnimationKeyframe)));
                writtenFrames++;
                
            }
            
        }

        if (writtenFrames < totalKeyFrames)
            throw new Exception("Wrote fewer keyframes than expected! Something is wrong!");

        writer.Write("END OF ANIMATION");
        
        writer.Flush();
        writer.Close();
    }

    private GLTF.Animation SelectAnimation(string? name, GLTFFile loader)
    {
        if (loader.Animations.Length == 0)
            throw new Exception("GLB has no animations!");

        if (name == null)
            return loader.Animations[0];

        for (int i = 0; i < loader.Animations.Length; i++)
        {
            if (loader.Animations[i].Name == name)
                return loader.Animations[i];
        }
        
        throw new Exception("GLB has no animations with the name '" + name + "'");
    }

    public override string GetCompiledPath(string path)
    {
        return Path.ChangeExtension(path, "anim");
    }
    
    private struct AnimDef()
    {
        [JsonProperty("glb", Required = Required.Always)] public string Glb;
        [JsonProperty("animation", Required = Required.AllowNull)] public string? Animation;
        [JsonProperty("name", Required = Required.Default)] public string? AnimationName;
        [JsonProperty("streamed", Required = Required.Default)] public bool Streamed;
    }
    
    private struct KeyframeData
    {
        public float Timestamp;
        public int Channel;
        public float[] Data;
    }
}