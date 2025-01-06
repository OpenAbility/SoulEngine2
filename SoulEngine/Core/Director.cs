using SoulEngine.Data.NBT;
using SoulEngine.Props;

namespace SoulEngine.Core;

/// <summary>
/// Steers all actors and props in a scene
/// </summary>
public abstract class Director
{
    private readonly Dictionary<string, SerializedProperty> properties = new Dictionary<string, SerializedProperty>();

    
    protected T Register<T>(T property) where T : SerializedProperty
    {
        properties[property.Name] = property;
        return property;
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
        CompoundTag tag = new CompoundTag("director");
        
        foreach (var p in properties)
        {
            tag[p.Key] = p.Value.Save();
        }

        return tag;
    }
    
    
    /// <summary>
    /// Load this director from file
    /// </summary>
    /// <param name="tag">The serialized tag</param>
    public abstract void OnLoad(CompoundTag tag);
    /// <summary>
    /// Load this director from file
    /// </summary>
    /// <param name="tag"></param>
    public abstract void OnSave(CompoundTag tag);

    /// <summary>
    /// Resets the scene
    /// </summary>
    public virtual void Reset()
    {
        
    }
    
    /// <summary>
    /// Edits this director
    /// </summary>
    public void Edit()
    {
        foreach (var p in properties.Values)
        {
            p.Edit();
        }
        
        OnEdit();
    }

    /// <summary>
    /// Called just after Edit
    /// </summary>
    protected virtual void OnEdit()
    {
        
    }
    
    
    public abstract void Update(float deltaTime);
}