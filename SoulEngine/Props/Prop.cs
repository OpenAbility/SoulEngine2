using System.Reflection;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using SoulEngine.Core;
using SoulEngine.Data.NBT;
using SoulEngine.Rendering;

namespace SoulEngine.Props;

/// <summary>
/// Scene object such as some static geometry or a moving chair.
/// </summary>
public abstract class Prop
{
    /// <summary>
    /// The name of the property
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The scene that this prop is in
    /// </summary>
    public readonly Scene Scene;

    public readonly string Type;

    private readonly Dictionary<string, SerializedProperty> properties = new Dictionary<string, SerializedProperty>();

    /// <summary>
    /// Create a new prop
    /// </summary>
    /// <param name="scene">The scene to create the prop in</param>
    /// <param name="type">The prop type</param>
    /// <param name="name">The name of the property</param>
    public Prop(Scene scene, string type, string name)
    {
        Scene = scene;
        Name = name;
        Type = type;
    }

    /// <summary>
    /// Loads this prop from the serialized tag
    /// </summary>
    /// <param name="tag">The tag to load from</param>
    public void Load(CompoundTag tag)
    {
        foreach (var p in properties)
        {
            if (tag.TryGetValue(p.Key, out Tag? propertyTag))
            {
                p.Value.Load(propertyTag);
                p.Value.MakeCurrentReset();
            }
        }
        
        OnLoad(tag);
    }

    /// <summary>
    /// Saves this prop to a tag
    /// </summary>
    /// <returns>The tag to save this prop as</returns>
    public CompoundTag Save()
    {
        CompoundTag tag = new CompoundTag(Name);
        
        tag.SetString("$_type", Type);
        
        foreach (var p in properties)
        {
            tag[p.Key] = p.Value.Save();
        }

        OnSave(tag);

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
    /// Called after the prop is finished loading
    /// </summary>
    /// <param name="tag">The tag that we loaded from</param>
    public virtual void OnSave(CompoundTag tag)
    {
        
    }
    
    /// <summary>
    /// Update inner prop logic. Do not update render logic!
    /// </summary>
    /// <param name="deltaTime">The time that has passed since last update</param>
    public virtual void Update(float deltaTime)
    {
        
    }

    /// <summary>
    /// Render this prop. Do not update non-render logic!
    /// </summary>
    /// <param name="sceneRenderData">The scene render data</param>
    /// <param name="deltaTime">The time that has passed since last render</param>
    public virtual void Render(SceneRenderData sceneRenderData, float deltaTime)
    {
        
    }

    public void Reset()
    {
        foreach (var p in properties.Values)
        {
            p.Reset();
        }
        OnReset();
    }
    
    protected virtual void OnReset()
    {
        
    }
    
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
        
        OnEdit();
    }

    protected virtual void OnEdit()
    {
        
    }
    
    
    
    protected T Register<T>(T property) where T : SerializedProperty
    {
        properties[property.Name] = property;
        return property;
    }
}