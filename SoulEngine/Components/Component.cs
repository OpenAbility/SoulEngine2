using System.Reflection;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;
using SoulEngine.Core;
using SoulEngine.Data.NBT;
using SoulEngine.Props;
using SoulEngine.Renderer;
using SoulEngine.Rendering;

namespace SoulEngine.Components;

public abstract class Component : EngineObject
{
    private readonly Dictionary<string, SerializedProperty> properties = new Dictionary<string, SerializedProperty>();
    
    public readonly string Type;

    public Entity Entity { get; }
    public Scene Scene => Entity.Scene;
    public Game Game => Entity.Scene.Game;

    internal Texture? propIcon;
    
    public Component(Entity entity)
    {
        Entity = entity;
        
        Type = GetType().GetCustomAttribute<ComponentAttribute>()?.ID ?? GetType().ToString();
        
        PropertyReflection.RegisterProperties(Entity.Scene, this, p => Register(p));
        
        
#if DEVELOPMENT
        string icon = GetType().GetCustomAttribute<PropAttribute>()?.Icon ?? "object";

        if(icon != "none")
            propIcon = Scene.Game.ResourceManager.Load<Texture>("icons/" + icon + ".png");
#endif
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
        CompoundTag tag = new CompoundTag(null);
        
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
    /// <param name="renderPipeline"></param>
    /// <param name="sceneRenderData">The scene render data</param>
    /// <param name="deltaTime">The time that has passed since last render</param>
    public virtual void Render(IRenderPipeline renderPipeline, SceneRenderData sceneRenderData, float deltaTime)
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
        ImGui.PushID("Component" + ObjectID);

        if (!ImGui.CollapsingHeader(Type))
        {
            ImGui.PopID();
            return;
        }
            
        
        ImGui.Indent();
        
        foreach (var p in properties.Values)
        {
            p.Edit();
        }
        
        OnEdit();

        if (ImGui.Button("Detach"))
        {
            Entity.Detach(this);
        }
        
        ImGui.Unindent();
        
        ImGui.PopID();
    }

    protected virtual void OnEdit()
    {
        
    }

    public virtual void RenderGizmo(GizmoContext context)
    {
        
    }

    protected T Register<T>(T property) where T : SerializedProperty
    {
        properties[property.Name] = property;
        return property;
    }
}