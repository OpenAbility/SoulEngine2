using ImGuiNET;
using Newtonsoft.Json.Linq;
using SoulEngine.Core;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

/// <summary>
/// Scene object such as some static geometry or a moving chair.
/// </summary>
public abstract class Prop
{
    /// <summary>
    /// The prop type string
    /// </summary>
    public readonly string Type;

    /// <summary>
    /// The name of the property
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The scene that this prop is in
    /// </summary>
    public readonly Scene Scene;

    private readonly Dictionary<string, PropProperty> properties = new Dictionary<string, PropProperty>();

    /// <summary>
    /// Create a new prop
    /// </summary>
    /// <param name="scene">The scene to create the prop in</param>
    /// <param name="type">The prop type</param>
    /// <param name="name">The name of the property</param>
    public Prop(Scene scene, string type, string name)
    {
        Scene = scene;
        Type = type;
        Name = name;
    }

    /// <summary>
    /// Loads this prop from the serialized tag
    /// </summary>
    /// <param name="tag">The tag to load from</param>
    public void Load(CompoundTag tag)
    {
        foreach (var p in properties)
        {
            if(tag.TryGetValue(p.Key, out Tag? propertyTag))
                p.Value.Load(propertyTag);
        }
    }

    /// <summary>
    /// Saves this prop to a tag
    /// </summary>
    /// <returns>The tag to save this prop as</returns>
    public CompoundTag Save()
    {
        CompoundTag tag = new CompoundTag(Name);
        
        foreach (var p in properties)
        {
            tag[p.Key] = p.Value.Save();
        }

        return tag;
    }


    /// <summary>
    /// Called after the prop is finished loading
    /// </summary>
    /// <param name="tag">The tag that we loaded from</param>
    public virtual void OnLoad(CompoundTag tag)
    {
        
    }
    
    /// <summary>
    /// Update inner prop logic
    /// </summary>
    /// <param name="deltaTime">The time that has passed since last update</param>
    public virtual void Update(float deltaTime)
    {
        
    }
    
    #if DEVELOPMENT

    public void Edit()
    {
        string n = Name;
        if (ImGui.InputText("name", ref n, 2048))
            Name = n;
        ImGui.TextDisabled(Type);
        ImGui.SeparatorText("properties");
        
        foreach (var p in properties.Values)
        {
            p.Edit();
        }
    }
    
    #endif
    
    
    protected T Register<T>(T property) where T : PropProperty
    {
        properties[property.Name] = property;
        return property;
    }
}