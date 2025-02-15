using System.Drawing;
using OpenTK.Mathematics;
using SoulEngine.Content;
using SoulEngine.Data.NBT;
using SoulEngine.Rendering;
using SoulEngine.Resources;

namespace SoulEngine.UI;

[Resource(typeof(SpriteLoader))]
public class Sprite : Resource
{
    public readonly Texture Texture;
    public readonly Vector2 Position;
    public readonly Vector2 Size;
    
    public Sprite(Texture texture, Vector2i pos, Vector2i size)
    {
        Texture = texture;
        Position = pos;
        Size = size;
    }
    
    
}


public class SpriteLoader : IResourceLoader<Sprite>
{
    public Sprite LoadResource(ResourceManager resourceManager, string id, ContentContext content)
    {
        CompoundTag tag = (CompoundTag)TagIO.ReadCompressed(content.Load(id)!);

        string mode = tag.GetString("mode") ?? "single";
        Texture? texture = null;
        Rectangle rectangle = new Rectangle(0, 0, 0, 0);


        if (mode == "single")
        {
            texture = resourceManager.Load<Texture>(tag.GetString("texture")!);
            rectangle = new Rectangle(0, 0, texture.Width, texture.Height);

            if (tag.ContainsKey("rect"))
            {
                CompoundTag rect = tag.GetTag<CompoundTag>("rect")!;
                rectangle.X = rect.GetInt("x")!.Value;
                rectangle.Y = rect.GetInt("y")!.Value;
                rectangle.Width = rect.GetInt("w")!.Value;
                rectangle.Height = rect.GetInt("h")!.Value;
            }
        }
        else
        {
            throw new Exception("Unknown texture mode " + mode);
        }

        return new Sprite(texture!, new Vector2i(rectangle.X, rectangle.Y), new Vector2i(rectangle.Width, rectangle.Height));
    }
}