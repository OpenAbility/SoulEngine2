using System.Numerics;
using Hexa.NET.ImGui;
using SoulEngine.Components;
using SoulEngine.Data.NBT;
using SoulEngine.Props;
using SoulEngine.Renderer;
using SoulEngine.Rendering;

namespace SoulEngine.Core;

[Prop("entity")]
[Serializable]
public class Entity : Prop
{
    private readonly List<Component> components = new List<Component>();
    
    public Entity(Scene scene, string type, string name) : base(scene, type, name)
    {
    }
    
    

    public void Attach(Component component)
    {
        components.Add(component);
    }

    public T? GetComponent<T>() where T : Component
    {
        return components.FirstOrDefault(c => c is T) as T;
    }
    
    public IEnumerable<T> GetComponents<T>() where T : Component
    {
        return components.Where(c => c is T).Cast<T>();
    }

    public override void OnSave(CompoundTag tag)
    {
        ListTag componentsTag = new ListTag("components");
        tag.Add(componentsTag);

        foreach (var component in components.ToArray())
        {
            componentsTag.Add(component.Save());
        }
    }

    public override void OnLoad(CompoundTag tag)
    {

        ListTag componentsTag = tag.GetTag<ListTag>("components")!;

        List<(Component component, CompoundTag tag)> tags = new List<(Component component, CompoundTag tag)>();
        
        foreach (var componentTag in componentsTag.Cast<CompoundTag>())
        {
            string componentType = componentTag.GetString("$_type")!;
            Component component = ComponentLoader.Create(this, componentType);
            components.Add(component);
            
            tags.Add((component, componentTag));
            
            
        }

        foreach (var ctag in tags)
        {
            ctag.component.Load(ctag.tag);
        }
    }

    public override void Render(IRenderPipeline renderPipeline, SceneRenderData sceneRenderData, float deltaTime)
    {
        foreach (var component in components.ToArray())
        {
            component.Render(renderPipeline, sceneRenderData, deltaTime);
        }
    }

    public override void Update(float deltaTime)
    {
        foreach (var component in components.ToArray())
        {
            component.Update(deltaTime);
        }
    }

    protected override void OnEdit()
    {
        ImGui.SeparatorText("Components");
        foreach (var component in components.ToArray())
        {
            component.Edit();
        }
        
        if (ImGui.BeginPopupContextWindow())
        {
            foreach (var component in ComponentLoader.Types)
            {
                if (ImGui.Selectable(component))
                {
                    components.Add(ComponentLoader.Create(this, component));
                }
            }
                    
            ImGui.EndPopup();
        }
    }

    protected override void OnReset()
    {
        foreach (var component in components.ToArray())
        {
            component.Reset();
        }
    }

    public override void RenderGizmo(GizmoContext context)
    {
        base.RenderGizmo(context);
        foreach (var component in components.ToArray())
        {
            component.RenderGizmo(context);
        }
    }

    public void Detach(Component component)
    {
        components.Remove(component);
    }

    public int IndexOfComponent(Component value)
    {
        return components.IndexOf(value);
    }

    public Component IndexedComponent(int index)
    {
        return components[index];
    }
}